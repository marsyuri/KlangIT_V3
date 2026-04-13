using KlangIT_V3.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KlangIT_V3.ViewModels
{
    public class ItemDetailViewModel
    {
        public int Id { get; set; }

        public string AssetId { get; set; } = string.Empty;

        public string AssetId1 { get; set; } = string.Empty;

        public string AssetId2 { get; set; } = string.Empty;

        public string AssetId3 { get; set; } = string.Empty;

        public string AssetId4 { get; set; } = string.Empty;

        public string SerialNo { get; set; } = string.Empty;

        public int SelectedItemTypeId { get; set; }

        public List<SelectListItem> ItemTypes { get; set; } = new();

        public int SelectedItemBrandId { get; set; }
        public List<SelectListItem> ItemBrands { get; set; } = new();

        public int SelectedItemModelId { get; set; }
        public List<SelectListItem> ItemModels { get; set; } = new();

        public string? ItemDescription { get; set; } = string.Empty;

        public string? ItemImageUrl { get; set; }

        public int TotalQuantity { get; set; }

        public int StockBalance { get; set; }

        public int StockUsed { get; set; }

        public int MinimumQuantity { get; set; }

        public int SelectedItemStatusId { get; set; }
        public List<SelectListItem> ItemStatuses { get; set; } = new();

        public string? Remarks { get; set; } = string.Empty;
        
    }
}
