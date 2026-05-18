using KlangIT_V3.Data;
using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Services
{
    public class ItemService : IItemService
    {
        private readonly ItLptWarehouseContext _db;
        private readonly IStockService _stockService;

        public ItemService(ItLptWarehouseContext db, IStockService stockService)
        {
            _db = db;
            _stockService = stockService;
        }

        // ── Index ─────────────────────────────────────────────────────────────────
        public async Task<IReadOnlyList<Item>> GetFilteredItemsAsync(
            string? sortOrder,
            int? typeId,
            int? brandId,
            int? modelId,
            string? search)
        {
            var query = _db.Items
                .Include(i => i.ItemBrand).Include(i => i.ItemModel).Include(i => i.ItemType)
                .Where(i => !i.IsDeleted)
                .AsQueryable();

            if (typeId.HasValue)  query = query.Where(i => i.ItemTypeId  == typeId.Value);
            if (brandId.HasValue) query = query.Where(i => i.ItemBrandId == brandId.Value);
            if (modelId.HasValue) query = query.Where(i => i.ItemModelId == modelId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";
                query = query.Where(i =>
                    EF.Functions.Like(i.AssetId ?? "", pattern) ||
                    EF.Functions.Like(i.SerialNo ?? "", pattern) ||
                    EF.Functions.Like(i.ItemType.Name, pattern) ||
                    EF.Functions.Like(i.ItemBrand.Name, pattern) ||
                    EF.Functions.Like(i.ItemModel!.Name, pattern));
            }

            query = sortOrder switch
            {
                "asset_asc"   => query.OrderBy(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenBy(s => s.AssetId),
                "asset_desc"  => query.OrderByDescending(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenByDescending(s => s.AssetId),
                "type_asc"    => query.OrderBy(s => s.ItemType.Name).ThenBy(s => s.AssetId),
                "type_desc"   => query.OrderByDescending(s => s.ItemType.Name).ThenByDescending(s => s.AssetId),
                "brand_asc"   => query.OrderBy(s => s.ItemBrand.Name).ThenBy(s => s.ItemModel!.Name),
                "brand_desc"  => query.OrderByDescending(s => s.ItemBrand.Name).ThenByDescending(s => s.ItemModel!.Name),
                "model_asc"   => query.OrderBy(s => s.ItemModel!.Name).ThenBy(s => s.ItemBrand.Name),
                "model_desc"  => query.OrderByDescending(s => s.ItemModel!.Name).ThenByDescending(s => s.ItemBrand.Name),
                "avl_asc"     => query.OrderBy(s => s.AvailableAmount),
                "avl_desc"    => query.OrderByDescending(s => s.AvailableAmount),
                "moddate_asc" => query.OrderBy(s => s.ModifiedDate),
                "moddate_desc"=> query.OrderByDescending(s => s.ModifiedDate),
                _             => query.OrderBy(s => s.AssetId)
            };

            return await query.ToListAsync();
        }

        // ── Lookups ───────────────────────────────────────────────────────────────
        public async Task<List<ItemType>> GetActiveItemTypesAsync()
            => await _db.ItemTypes.Where(t => !t.IsDeleted).OrderBy(t => t.Name).ToListAsync();

        public async Task<List<ItemBrand>> GetActiveItemBrandsAsync()
            => await _db.ItemBrands.Where(b => !b.IsDeleted).OrderBy(b => b.Name).ToListAsync();

        public async Task<List<ItemModel>> GetActiveItemModelsAsync()
            => await _db.ItemModels.Where(m => !m.IsDeleted).OrderBy(m => m.Name).ToListAsync();

        public async Task<ItemCascadeMaps> GetCascadeMapsAsync()
        {
            var links = await _db.ItemTypeToBrands.ToListAsync();
            var models = await _db.ItemModels.Where(m => !m.IsDeleted).OrderBy(m => m.Name).ToListAsync();

            return new ItemCascadeMaps
            {
                TypeToBrands  = links.GroupBy(l => l.ItemTypeId).ToDictionary(g => g.Key, g => g.Select(l => l.ItemBrandId).ToList()),
                BrandToTypes  = links.GroupBy(l => l.ItemBrandId).ToDictionary(g => g.Key, g => g.Select(l => l.ItemTypeId).ToList()),
                BrandToModels = models.GroupBy(m => m.ItemBrandId).ToDictionary(g => g.Key, g => g.ToList())
            };
        }

        // ── Reads ─────────────────────────────────────────────────────────────────
        public async Task<Item?> GetItemDetailsAsync(int id)
        {
            var item = await _db.Items
                .Include(i => i.ItemBrand).Include(i => i.ItemModel).Include(i => i.ItemType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return null;

            item.BorrowHistories = await _db.BorrowHistories
                .Include(b => b.BorrowerDepartment).Include(b => b.BorrowerSection)
                .Where(b => b.ItemId == item.Id && !b.IsDeleted)
                .ToListAsync();

            return item;
        }

        public async Task<List<StockLog>> GetStockLogsAsync(int itemId)
            => await _db.StockLogs
                .Where(sl => sl.ItemId == itemId)
                .OrderByDescending(sl => sl.CreatedDate)
                .ThenByDescending(sl => sl.Id)
                .ToListAsync();

        public async Task<Item?> GetItemWithRelationsAsync(int id)
            => await _db.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<Item?> GetItemAsync(int id)
            => await _db.Items.FindAsync(id);

        public async Task<int> GetItemCountAsync()
            => await _db.Items.CountAsync();

        public async Task<Item?> GetForDeleteAsync(int id)
            => await _db.Items
                .Include(i => i.ItemBrand).Include(i => i.ItemModel).Include(i => i.ItemType)
                .Include(i => i.BorrowHistories)
                .FirstOrDefaultAsync(m => m.Id == id);

        // ── Mutations ─────────────────────────────────────────────────────────────
        public async Task<Item> CreateItemAsync(Item newItem, int initialAmount, ItemStatusEnum selectedStatus, string username)
        {
            newItem.TotalAmount = 0;
            newItem.ActiveAmount = 0;
            newItem.AvailableAmount = 0;
            newItem.BorrowedAmount = 0;
            newItem.DamagedAmount = 0;
            newItem.DisposedAmount = 0;
            newItem.ItemStatus = (int)selectedStatus;
            newItem.CreatedBy = username;
            newItem.ModifiedBy = username;
            newItem.CreatedDate = DateTime.Now;
            newItem.ModifiedDate = DateTime.Now;
            newItem.IsDeleted = false;

            _db.Items.Add(newItem);
            await _db.SaveChangesAsync();

            var initialLogType = selectedStatus switch
            {
                ItemStatusEnum.Available => StockLogTypeEnum.InitialAvailable,
                ItemStatusEnum.Borrowed  => StockLogTypeEnum.InitialBorrowed,
                ItemStatusEnum.Damaged   => StockLogTypeEnum.InitialDamaged,
                ItemStatusEnum.Disposed  => StockLogTypeEnum.InitialDisposed,
                _ => throw new InvalidOperationException($"ไม่รองรับ ItemStatus: {selectedStatus}")
            };

            await _stockService.ApplyStockChangeAsync(
                newItem.Id,
                initialLogType,
                deltaAvailable: selectedStatus == ItemStatusEnum.Available ? initialAmount : 0,
                deltaBorrowed:  selectedStatus == ItemStatusEnum.Borrowed  ? initialAmount : 0,
                deltaDamaged:   selectedStatus == ItemStatusEnum.Damaged   ? initialAmount : 0,
                deltaDisposed:  selectedStatus == ItemStatusEnum.Disposed  ? initialAmount : 0,
                createdBy: username,
                remarks: $"รับเข้าเริ่มต้น - {selectedStatus.GetDisplayName()}");

            return newItem;
        }

        public async Task UpdateItemAsync(Item mutated, string username)
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
                if (!await ItemExistsAsync(mutated.Id)) throw new KeyNotFoundException($"Item {mutated.Id} ไม่พบ");
                throw;
            }
        }

        public async Task<DeleteOutcome> DeleteItemAsync(int id)
        {
            var item = await _db.Items
                .Include(i => i.BorrowHistories)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return DeleteOutcome.NotFound;

            if (item.BorrowHistories.Any(b => !b.IsDeleted))
                return DeleteOutcome.HasDependencies;

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return DeleteOutcome.Deleted;
        }

        public async Task MarkDamagedAsync(int itemId, int amount, string? remarks, string username, string itstaffDisplay)
        {
            var item = await _db.Items.FindAsync(itemId)
                ?? throw new KeyNotFoundException($"Item {itemId} ไม่พบ");

            if (amount > item.AvailableAmount)
                throw new InvalidOperationException("จำนวนเกินจำนวนพร้อมใช้");

            item.ItemStatus = (int)ItemStatusEnum.Damaged;

            await _stockService.ApplyStockChangeAsync(
                itemId,
                StockLogTypeEnum.Damage,
                deltaAvailable: -amount,
                deltaBorrowed:   0,
                deltaDamaged:   +amount,
                deltaDisposed:   0,
                createdBy:       username,
                remarks:         string.IsNullOrWhiteSpace(remarks) ? $"แจ้งเสียหาย โดย {itstaffDisplay}" : $"{remarks} (โดย {itstaffDisplay})");
        }

        public async Task MarkRepairedAsync(int itemId, int amount, string? remarks, string username, string itstaffDisplay)
        {
            var item = await _db.Items.FindAsync(itemId)
                ?? throw new KeyNotFoundException($"Item {itemId} ไม่พบ");

            if (amount > item.DamagedAmount)
                throw new InvalidOperationException("จำนวนเกินจำนวนที่ชำรุดอยู่");

            item.ItemStatus = (int)ItemStatusEnum.Available;

            await _stockService.ApplyStockChangeAsync(
                itemId,
                StockLogTypeEnum.Repair,
                deltaAvailable: +amount,
                deltaBorrowed:   0,
                deltaDamaged:   -amount,
                deltaDisposed:   0,
                createdBy:       username,
                remarks:         string.IsNullOrWhiteSpace(remarks) ? $"ซ่อมแล้ว โดย {itstaffDisplay}" : $"{remarks} (โดย {itstaffDisplay})");
        }

        public async Task<bool> ItemExistsAsync(int id)
            => await _db.Items.AnyAsync(e => e.Id == id);
    }
}
