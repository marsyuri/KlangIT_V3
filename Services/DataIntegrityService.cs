using KlangIT_V3.Data;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Services
{
    public class DataIntegrityService : IDataIntegrityService
    {
        private readonly ItLptWarehouseContext _db;

        public DataIntegrityService(ItLptWarehouseContext db)
        {
            _db = db;
        }

        public async Task<DataIntegrityReport> RunChecksAsync()
        {
            var activeItems = await _db.Items.Where(i => !i.IsDeleted).ToListAsync();

            // Check 1: Internal math
            var internalMath = activeItems
                .Where(i => i.ActiveAmount != (i.AvailableAmount + i.BorrowedAmount + i.DamagedAmount)
                         || i.TotalAmount  != (i.AvailableAmount + i.BorrowedAmount + i.DamagedAmount + i.DisposedAmount))
                .ToList();

            // Check 4: Negative amounts
            var negative = activeItems
                .Where(i => i.AvailableAmount < 0
                         || i.BorrowedAmount  < 0
                         || i.DamagedAmount   < 0
                         || i.DisposedAmount  < 0
                         || i.TotalAmount     < 0
                         || i.ActiveAmount    < 0)
                .ToList();

            // Check 2: Item amounts vs latest StockLog
            var latestLogs = await _db.StockLogs
                .GroupBy(sl => sl.ItemId)
                .Select(g => g.OrderByDescending(sl => sl.CreatedDate)
                              .ThenByDescending(sl => sl.Id)
                              .First())
                .ToListAsync();
            var latestByItem = latestLogs.ToDictionary(sl => sl.ItemId);

            var amountVsLog = new List<ItemStockLogMismatch>();
            foreach (var item in activeItems)
            {
                latestByItem.TryGetValue(item.Id, out var log);
                if (log == null)
                {
                    amountVsLog.Add(new ItemStockLogMismatch(item, null));
                    continue;
                }
                if (item.AvailableAmount != log.AvailableAfter
                 || item.BorrowedAmount  != log.BorrowedAfter
                 || item.DamagedAmount   != log.DamagedAfter
                 || item.DisposedAmount  != log.DisposedAfter
                 || item.TotalAmount     != log.TotalAfter)
                {
                    amountVsLog.Add(new ItemStockLogMismatch(item, log));
                }
            }

            // Check 3: BorrowedAmount vs active BorrowHistory
            var activeBhByItem = await _db.BorrowHistories
                .Where(bh => bh.ReturnDate == null && !bh.IsDeleted)
                .GroupBy(bh => bh.ItemId)
                .Select(g => new { ItemId = g.Key, Sum = g.Sum(bh => bh.Amount), Count = g.Count() })
                .ToListAsync();
            var bhMap = activeBhByItem.ToDictionary(x => x.ItemId, x => (x.Sum, x.Count));

            var borrowMismatch = new List<ItemBorrowMismatch>();
            foreach (var item in activeItems)
            {
                bhMap.TryGetValue(item.Id, out var bh);
                if (item.BorrowedAmount != bh.Sum)
                    borrowMismatch.Add(new ItemBorrowMismatch(item, bh.Sum, bh.Count));
            }

            // Check 5: StockLog continuity (After = previous.After + Delta)
            var allLogs = await _db.StockLogs
                .OrderBy(sl => sl.ItemId)
                .ThenBy(sl => sl.CreatedDate)
                .ThenBy(sl => sl.Id)
                .ToListAsync();

            var continuityIssues = new List<StockLogContinuityGap>();
            StockLog? prev = null;
            foreach (var log in allLogs)
            {
                if (prev != null && prev.ItemId == log.ItemId)
                {
                    if (prev.AvailableAfter + log.DeltaAvailable != log.AvailableAfter
                     || prev.BorrowedAfter  + log.DeltaBorrowed  != log.BorrowedAfter
                     || prev.DamagedAfter   + log.DeltaDamaged   != log.DamagedAfter
                     || prev.DisposedAfter  + log.DeltaDisposed  != log.DisposedAfter)
                    {
                        continuityIssues.Add(new StockLogContinuityGap(log, prev));
                    }
                }
                prev = log;
            }

            return new DataIntegrityReport
            {
                InternalMathIssues       = internalMath,
                NegativeAmountIssues     = negative,
                AmountVsStockLogIssues   = amountVsLog,
                BorrowedVsActiveBhIssues = borrowMismatch,
                StockLogContinuityIssues = continuityIssues
            };
        }

        // ── Reset to Initial state ────────────────────────────────────────────────
        public async Task<ResetPreview> GetResetPreviewAsync()
        {
            int bhCount = await _db.BorrowHistories.CountAsync();
            int nonInitLogCount = await _db.StockLogs.CountAsync(sl => sl.LogType > 4);

            int itemsWithLog = await _db.Items
                .Where(i => !i.IsDeleted && i.StockLogs.Any())
                .CountAsync();

            int itemsWithoutLog = await _db.Items
                .Where(i => !i.IsDeleted && !i.StockLogs.Any())
                .CountAsync();

            int orphanBorrowed = await _db.Items
                .Where(i => !i.IsDeleted && i.BorrowedAmount > 0)
                .CountAsync();

            return new ResetPreview
            {
                BorrowHistoryCount      = bhCount,
                NonInitialStockLogCount = nonInitLogCount,
                ItemsToRecomputeCount   = itemsWithLog,
                ItemsWithoutLogCount    = itemsWithoutLog,
                OrphanBorrowedCount     = orphanBorrowed
            };
        }

        public async Task<ResetResult> ResetToInitialAsync(string username, bool normalizeOrphanBorrowed)
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            // 1. ลบ BorrowHistory ทั้งหมด
            int bhDeleted = await _db.BorrowHistories.ExecuteDeleteAsync();

            // 2. ลบ StockLog ที่ไม่ใช่ Initial (LogType > 4)
            int logsDeleted = await _db.StockLogs.Where(sl => sl.LogType > 4).ExecuteDeleteAsync();

            // 3. Recompute Item.Amounts จาก StockLog ล่าสุดที่เหลือ (= Initial row)
            var latestLogs = await _db.StockLogs
                .GroupBy(sl => sl.ItemId)
                .Select(g => g.OrderByDescending(sl => sl.CreatedDate)
                              .ThenByDescending(sl => sl.Id)
                              .First())
                .ToListAsync();
            var latestByItem = latestLogs.ToDictionary(sl => sl.ItemId);

            var activeItems = await _db.Items.Where(i => !i.IsDeleted).ToListAsync();
            int recomputed = 0, resetZero = 0;

            foreach (var item in activeItems)
            {
                if (latestByItem.TryGetValue(item.Id, out var log))
                {
                    item.AvailableAmount = log.AvailableAfter;
                    item.BorrowedAmount  = log.BorrowedAfter;
                    item.DamagedAmount   = log.DamagedAfter;
                    item.DisposedAmount  = log.DisposedAfter;
                    item.ActiveAmount    = log.AvailableAfter + log.BorrowedAfter + log.DamagedAfter;
                    item.TotalAmount     = log.TotalAfter;
                    recomputed++;
                }
                else
                {
                    item.AvailableAmount = 0;
                    item.BorrowedAmount  = 0;
                    item.DamagedAmount   = 0;
                    item.DisposedAmount  = 0;
                    item.ActiveAmount    = 0;
                    item.TotalAmount     = 0;
                    resetZero++;
                }
                item.ModifiedBy   = username;
                item.ModifiedDate = DateTime.Now;
            }
            await _db.SaveChangesAsync();

            // 4. (optional) Pattern B normalize: ย้าย BorrowedAmount → AvailableAmount + Adjust log
            int orphanNormalized = 0;
            if (normalizeOrphanBorrowed)
            {
                var orphans = activeItems.Where(i => i.BorrowedAmount > 0).ToList();
                foreach (var item in orphans)
                {
                    int amount = item.BorrowedAmount;
                    item.AvailableAmount += amount;
                    item.BorrowedAmount   = 0;
                    item.ItemStatus       = (int)ItemStatusEnum.Available;
                    item.ModifiedBy       = username;
                    item.ModifiedDate     = DateTime.Now;

                    _db.StockLogs.Add(new StockLog
                    {
                        ItemId         = item.Id,
                        LogType        = (int)StockLogTypeEnum.Adjust,
                        DeltaAvailable = +amount,
                        DeltaBorrowed  = -amount,
                        DeltaDamaged   = 0,
                        DeltaDisposed  = 0,
                        DeltaTotal     = 0,
                        AvailableAfter = item.AvailableAmount,
                        BorrowedAfter  = item.BorrowedAmount,
                        DamagedAfter   = item.DamagedAmount,
                        DisposedAfter  = item.DisposedAmount,
                        TotalAfter     = item.TotalAmount,
                        Remarks        = "Reset: ปรับ BorrowedAmount → AvailableAmount เพราะไม่มี BH รองรับ",
                        CreatedDate    = DateTime.Now,
                        CreatedBy      = username
                    });
                    orphanNormalized++;
                }
                await _db.SaveChangesAsync();
            }

            await tx.CommitAsync();

            return new ResetResult
            {
                BorrowHistoriesDeleted   = bhDeleted,
                StockLogsDeleted         = logsDeleted,
                ItemsRecomputed          = recomputed,
                ItemsResetToZero         = resetZero,
                OrphanBorrowedNormalized = orphanNormalized
            };
        }
    }
}
