using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Helpers
{
    public static class StockLogBackfillHelper
    {
        public static async Task<(int itemsProcessed, int logsCreated)> BackfillOpeningBalancesAsync(
            ItLptWarehouseContext db,
            string createdBy)
        {
            var itemIds = await db.Items
                .Where(i => !i.IsDeleted && !i.StockLogs.Any())
                .Select(i => i.Id)
                .ToListAsync();

            int itemsProcessed = 0;
            int logsCreated    = 0;
            var now            = DateTime.Now;

            foreach (var itemId in itemIds)
            {
                var item = await db.Items.FindAsync(itemId);
                if (item == null) continue;

                int availAfter = 0, borrAfter = 0, damAfter = 0, dispAfter = 0;

                if (item.AvailableAmount > 0)
                {
                    availAfter = item.AvailableAmount;
                    db.StockLogs.Add(new StockLog
                    {
                        ItemId         = item.Id,
                        LogType        = (int)StockLogTypeEnum.InitialAvailable,
                        DeltaAvailable = item.AvailableAmount,
                        DeltaTotal     = item.AvailableAmount,
                        AvailableAfter = availAfter,
                        BorrowedAfter  = borrAfter,
                        DamagedAfter   = damAfter,
                        DisposedAfter  = dispAfter,
                        TotalAfter     = availAfter + borrAfter + damAfter + dispAfter,
                        Remarks        = "Backfill opening balance",
                        CreatedDate    = now,
                        CreatedBy      = createdBy
                    });
                    logsCreated++;
                }

                if (item.BorrowedAmount > 0)
                {
                    borrAfter = item.BorrowedAmount;
                    db.StockLogs.Add(new StockLog
                    {
                        ItemId         = item.Id,
                        LogType        = (int)StockLogTypeEnum.InitialBorrowed,
                        DeltaBorrowed  = item.BorrowedAmount,
                        DeltaTotal     = item.BorrowedAmount,
                        AvailableAfter = availAfter,
                        BorrowedAfter  = borrAfter,
                        DamagedAfter   = damAfter,
                        DisposedAfter  = dispAfter,
                        TotalAfter     = availAfter + borrAfter + damAfter + dispAfter,
                        Remarks        = "Backfill opening balance",
                        CreatedDate    = now,
                        CreatedBy      = createdBy
                    });
                    logsCreated++;
                }

                if (item.DamagedAmount > 0)
                {
                    damAfter = item.DamagedAmount;
                    db.StockLogs.Add(new StockLog
                    {
                        ItemId         = item.Id,
                        LogType        = (int)StockLogTypeEnum.InitialDamaged,
                        DeltaDamaged   = item.DamagedAmount,
                        DeltaTotal     = item.DamagedAmount,
                        AvailableAfter = availAfter,
                        BorrowedAfter  = borrAfter,
                        DamagedAfter   = damAfter,
                        DisposedAfter  = dispAfter,
                        TotalAfter     = availAfter + borrAfter + damAfter + dispAfter,
                        Remarks        = "Backfill opening balance",
                        CreatedDate    = now,
                        CreatedBy      = createdBy
                    });
                    logsCreated++;
                }

                if (item.DisposedAmount > 0)
                {
                    dispAfter = item.DisposedAmount;
                    db.StockLogs.Add(new StockLog
                    {
                        ItemId         = item.Id,
                        LogType        = (int)StockLogTypeEnum.InitialDisposed,
                        DeltaDisposed  = item.DisposedAmount,
                        DeltaTotal     = item.DisposedAmount,
                        AvailableAfter = availAfter,
                        BorrowedAfter  = borrAfter,
                        DamagedAfter   = damAfter,
                        DisposedAfter  = dispAfter,
                        TotalAfter     = availAfter + borrAfter + damAfter + dispAfter,
                        Remarks        = "Backfill opening balance",
                        CreatedDate    = now,
                        CreatedBy      = createdBy
                    });
                    logsCreated++;
                }

                itemsProcessed++;
            }

            await db.SaveChangesAsync();
            return (itemsProcessed, logsCreated);
        }
    }
}
