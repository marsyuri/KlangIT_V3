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

    public class ResetPreview
    {
        public int BorrowHistoryCount { get; init; }
        public int NonInitialStockLogCount { get; init; }
        public int ItemsToRecomputeCount { get; init; }
        public int ItemsWithoutLogCount { get; init; }
        public int OrphanBorrowedCount { get; init; }
    }

    public class ResetResult
    {
        public int BorrowHistoriesDeleted { get; init; }
        public int StockLogsDeleted { get; init; }
        public int ItemsRecomputed { get; init; }
        public int ItemsResetToZero { get; init; }
        public int OrphanBorrowedNormalized { get; init; }
    }

    public interface IDataIntegrityService
    {
        Task<DataIntegrityReport> RunChecksAsync();
        Task<ResetPreview> GetResetPreviewAsync();
        Task<ResetResult> ResetToInitialAsync(string username, bool normalizeOrphanBorrowed);
    }
}
