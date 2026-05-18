using KlangIT_V3.Models;

namespace KlangIT_V3.Services
{
    public class DataIntegrityReport
    {
        public List<Item> InternalMathIssues { get; init; } = new();
        public List<Item> NegativeAmountIssues { get; init; } = new();
        public List<ItemStockLogMismatch> AmountVsStockLogIssues { get; init; } = new();
        public List<ItemBorrowMismatch> BorrowedVsActiveBhIssues { get; init; } = new();
        public List<StockLogContinuityGap> StockLogContinuityIssues { get; init; } = new();

        public int TotalIssueCount =>
            InternalMathIssues.Count
          + NegativeAmountIssues.Count
          + AmountVsStockLogIssues.Count
          + BorrowedVsActiveBhIssues.Count
          + StockLogContinuityIssues.Count;

        public bool IsConsistent => TotalIssueCount == 0;
    }

    public record ItemStockLogMismatch(Item Item, StockLog? LatestLog);
    public record ItemBorrowMismatch(Item Item, int ActiveBorrowSum, int ActiveBorrowCount);
    public record StockLogContinuityGap(StockLog Current, StockLog Previous);

    public interface IDataIntegrityService
    {
        Task<DataIntegrityReport> RunChecksAsync();
    }
}
