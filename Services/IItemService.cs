using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;

namespace KlangIT_V3.Services
{
    public class ItemCascadeMaps
    {
        public Dictionary<int, List<int>> TypeToBrands { get; set; } = new();
        public Dictionary<int, List<int>> BrandToTypes { get; set; } = new();
        public Dictionary<int, List<ItemModel>> BrandToModels { get; set; } = new();
    }

    public enum DeleteOutcome
    {
        Deleted,
        NotFound,
        HasDependencies
    }

    public interface IItemService
    {
        Task<IReadOnlyList<Item>> GetFilteredItemsAsync(
            string? sortOrder,
            int? typeId,
            int? brandId,
            int? modelId,
            string? search);

        Task<List<ItemType>> GetActiveItemTypesAsync();
        Task<List<ItemBrand>> GetActiveItemBrandsAsync();
        Task<List<ItemModel>> GetActiveItemModelsAsync();
        Task<ItemCascadeMaps> GetCascadeMapsAsync();

        Task<int> GetItemCountAsync();

        Task<Item?> GetItemDetailsAsync(int id);
        Task<List<StockLog>> GetStockLogsAsync(int itemId);
        Task<Item?> GetItemWithRelationsAsync(int id);
        Task<Item?> GetItemAsync(int id);
        Task<Item?> GetForDeleteAsync(int id);

        Task<Item> CreateItemAsync(Item newItem, int initialAmount, ItemStatusEnum selectedStatus, string username);
        Task UpdateItemAsync(Item mutated, string username);
        Task<DeleteOutcome> DeleteItemAsync(int id);
        Task MarkDamagedAsync(int itemId, int amount, string? remarks, string username, string itstaffDisplay);
        Task MarkRepairedAsync(int itemId, int amount, string? remarks, string username, string itstaffDisplay);

        Task<bool> ItemExistsAsync(int id);
    }
}
