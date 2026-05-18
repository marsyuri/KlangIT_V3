using KlangIT_V3.Data;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly ItLptWarehouseContext _db;
        private readonly IStockService _stockService;

        public BorrowService(ItLptWarehouseContext db, IStockService stockService)
        {
            _db = db;
            _stockService = stockService;
        }

        // ── Reads ─────────────────────────────────────────────────────────────────
        public async Task<IReadOnlyList<BorrowHistory>> GetIndexAsync()
            => await _db.BorrowHistories
                .Include(b => b.Item).ThenInclude(i => i!.ItemType)
                .Include(b => b.Item).ThenInclude(i => i!.ItemBrand)
                .Include(b => b.BorrowerDepartment)
                .Include(b => b.BorrowerSection)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

        public async Task<BorrowHistory?> GetDetailsAsync(int id)
            => await _db.BorrowHistories
                .Include(b => b.Item).ThenInclude(i => i!.ItemType)
                .Include(b => b.Item).ThenInclude(i => i!.ItemBrand)
                .Include(b => b.Item).ThenInclude(i => i!.ItemModel)
                .Include(b => b.BorrowerDepartment)
                .Include(b => b.BorrowerSection)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<BorrowHistory?> GetActiveBorrowAsync(int id)
            => await _db.BorrowHistories
                .Where(b => b.Id == id && !b.IsDeleted && !b.ReturnDate.HasValue)
                .SingleOrDefaultAsync();

        public async Task<BorrowHistory?> GetForEditAsync(int id)
            => await _db.BorrowHistories.FindAsync(id);

        public async Task<BorrowHistory?> GetForDeleteAsync(int id)
            => await _db.BorrowHistories
                .Include(b => b.Item).ThenInclude(i => i!.ItemType)
                .Include(b => b.Item).ThenInclude(i => i!.ItemBrand)
                .Include(b => b.Item).ThenInclude(i => i!.ItemModel)
                .Include(b => b.BorrowerDepartment)
                .Include(b => b.BorrowerSection)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<List<Department>> GetActiveDepartmentsAsync()
            => await _db.Departments.Where(d => !d.IsDeleted).OrderBy(d => d.Name).ToListAsync();

        public async Task<List<Section>> GetActiveSectionsAsync()
            => await _db.Sections.Where(s => !s.IsDeleted).OrderBy(s => s.Name).ToListAsync();

        // ── Mutations ─────────────────────────────────────────────────────────────
        public async Task<BorrowHistory> CreateBorrowAsync(
            int itemId,
            string borrowerUser,
            int departmentId,
            int? sectionId,
            bool isPermanent,
            DateTime borrowDate,
            DateTime? dueDate,
            int amount,
            string displayName,
            string username)
        {
            var bh = new BorrowHistory
            {
                ItemId               = itemId,
                BorrowerUser         = borrowerUser,
                BorrowerDepartmentId = departmentId,
                BorrowerSectionId    = sectionId,
                IsPermanentBorrow    = isPermanent,
                IsInitial            = false,
                BorrowDate           = borrowDate,
                DueDate              = dueDate,
                BorrowItname         = displayName,
                ReturnItname         = string.Empty,
                Amount               = amount,
                CreatedDate          = DateTime.Now,
                ModifiedDate         = DateTime.Now,
                CreatedBy            = username,
                ModifiedBy           = username,
                IsDeleted            = false
            };
            _db.BorrowHistories.Add(bh);
            await _db.SaveChangesAsync();

            var item = await _db.Items.FindAsync(itemId)
                ?? throw new KeyNotFoundException($"Item {itemId} ไม่พบ");
            item.ItemStatus = (int)ItemStatusEnum.Borrowed;

            await _stockService.ApplyStockChangeAsync(
                itemId,
                StockLogTypeEnum.Borrow,
                deltaAvailable: -amount,
                deltaBorrowed:  +amount,
                deltaDamaged:   0,
                deltaDisposed:  0,
                createdBy:      username,
                referenceNo:    $"BH-{bh.Id}",
                remarks:        "ยืม");

            return bh;
        }

        public async Task<bool> ProcessReturnAsync(int bhId, int returnAmount, string displayName, string username)
        {
            var bh = await _db.BorrowHistories
                .Where(b => b.Id == bhId && !b.IsDeleted && !b.ReturnDate.HasValue)
                .SingleOrDefaultAsync();
            if (bh == null) return false;

            bool fullyReturned = bh.Amount == returnAmount;
            bh.IsPermanentBorrow = false;
            if (fullyReturned)
            {
                bh.ReturnDate   = DateTime.Now;
                bh.ReturnItname = displayName;
            }
            bh.Amount      -= returnAmount;
            bh.ModifiedBy   = username;
            bh.ModifiedDate = DateTime.Now;

            var item = await _db.Items.FindAsync(bh.ItemId)
                ?? throw new KeyNotFoundException($"Item {bh.ItemId} ไม่พบ");
            item.ItemStatus = (int)ItemStatusEnum.Available;

            await _stockService.ApplyStockChangeAsync(
                bh.ItemId,
                StockLogTypeEnum.Return,
                deltaAvailable: +returnAmount,
                deltaBorrowed:  -returnAmount,
                deltaDamaged:   0,
                deltaDisposed:  0,
                createdBy:      username,
                referenceNo:    $"BH-{bh.Id}",
                remarks:        fullyReturned ? "คืนทั้งหมด" : "คืนบางส่วน");

            return true;
        }

        public async Task UpdateBorrowAsync(BorrowHistory mutated, string username)
        {
            mutated.ModifiedBy = username;
            mutated.ModifiedDate = DateTime.Now;

            try
            {
                _db.Update(mutated);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BorrowHistoryExistsAsync(mutated.Id)) throw new KeyNotFoundException($"BorrowHistory {mutated.Id} ไม่พบ");
                throw;
            }
        }

        public async Task<bool> DeleteBorrowAsync(int id)
        {
            var bh = await _db.BorrowHistories.FindAsync(id);
            if (bh == null) return false;
            _db.BorrowHistories.Remove(bh);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BorrowHistoryExistsAsync(int id)
            => await _db.BorrowHistories.AnyAsync(e => e.Id == id);
    }
}
