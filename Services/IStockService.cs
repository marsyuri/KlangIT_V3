using KlangIT_V3.Models.Enums;

namespace KlangIT_V3.Services
{
    public interface IStockService
    {
        Task ApplyStockChangeAsync(
            int itemId,
            StockLogTypeEnum logType,
            int deltaAvailable,
            int deltaBorrowed,
            int deltaDamaged,
            int deltaDisposed,
            string createdBy,
            string? referenceNo = null,
            string? remarks = null);
    }
}
