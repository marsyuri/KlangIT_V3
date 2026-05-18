using KlangIT_V3.Data;
using KlangIT_V3.Models;
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
    }
}
