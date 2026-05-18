using KlangIT_V3.Data;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;

namespace KlangIT_V3.Services
{
    public class StockService : IStockService
    {
        private readonly ItLptWarehouseContext _db;

        public StockService(ItLptWarehouseContext db)
        {
            _db = db;
        }

        public async Task ApplyStockChangeAsync(
            int itemId,
            StockLogTypeEnum logType,
            int deltaAvailable,
            int deltaBorrowed,
            int deltaDamaged,
            int deltaDisposed,
            string createdBy,
            string? referenceNo = null,
            string? remarks = null)
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var item = await _db.Items.FindAsync(itemId)
                ?? throw new InvalidOperationException("ไม่พบ Item");

            item.AvailableAmount += deltaAvailable;
            item.BorrowedAmount += deltaBorrowed;
            item.DamagedAmount += deltaDamaged;
            item.DisposedAmount += deltaDisposed;
            item.ActiveAmount = item.AvailableAmount + item.BorrowedAmount + item.DamagedAmount;
            item.TotalAmount = item.ActiveAmount + item.DisposedAmount;
            item.ModifiedDate = DateTime.Now;
            item.ModifiedBy = createdBy;

            if (item.AvailableAmount < 0 || item.BorrowedAmount < 0
                || item.DamagedAmount < 0 || item.DisposedAmount < 0)
                throw new InvalidOperationException("จำนวนติดลบ");

            _db.StockLogs.Add(new StockLog
            {
                ItemId = itemId,
                LogType = (int)logType,
                DeltaAvailable = deltaAvailable,
                DeltaBorrowed = deltaBorrowed,
                DeltaDamaged = deltaDamaged,
                DeltaDisposed = deltaDisposed,
                DeltaTotal = deltaAvailable + deltaBorrowed + deltaDamaged + deltaDisposed,
                TotalAfter = item.TotalAmount,
                AvailableAfter = item.AvailableAmount,
                BorrowedAfter = item.BorrowedAmount,
                DamagedAfter = item.DamagedAmount,
                DisposedAfter = item.DisposedAmount,
                ReferenceNo = referenceNo,
                Remarks = remarks,
                CreatedDate = DateTime.Now,
                CreatedBy = createdBy
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
