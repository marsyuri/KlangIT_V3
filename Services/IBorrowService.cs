using KlangIT_V3.Models;

namespace KlangIT_V3.Services
{
    public interface IBorrowService
    {
        Task<IReadOnlyList<BorrowHistory>> GetIndexAsync();
        Task<BorrowHistory?> GetDetailsAsync(int id);
        Task<BorrowHistory?> GetActiveBorrowAsync(int id);
        Task<BorrowHistory?> GetForEditAsync(int id);
        Task<BorrowHistory?> GetForDeleteAsync(int id);

        Task<List<Department>> GetActiveDepartmentsAsync();
        Task<List<Section>> GetActiveSectionsAsync();

        Task<BorrowHistory> CreateBorrowAsync(
            int itemId,
            string borrowerUser,
            int departmentId,
            int? sectionId,
            bool isPermanent,
            DateTime borrowDate,
            DateTime? dueDate,
            int amount,
            string displayName,
            string username);

        Task<bool> ProcessReturnAsync(
            int bhId,
            int returnAmount,
            string displayName,
            string username);

        Task UpdateBorrowAsync(BorrowHistory mutated, string username);
        Task<bool> DeleteBorrowAsync(int id);
        Task<bool> BorrowHistoryExistsAsync(int id);
    }
}
