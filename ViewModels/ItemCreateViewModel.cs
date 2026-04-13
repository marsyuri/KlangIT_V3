using KlangIT_V3.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KlangIT_V3.ViewModels
{
    public class ItemCreateViewModel
    {
        // ── Asset ID (split into 4 parts) ──
        [NumericFixedLengthAttribute(4)]
        public string? AssetId1 { get; set; }

        [NumericFixedLengthAttribute(8)]
        public string? AssetId2 { get; set; }

        [NumericFixedLengthAttribute(8)]
        public string? AssetId3 { get; set; }

        [NumericFixedLengthAttribute(5)]
        public string? AssetId4 { get; set; }

        public string? OtherAssetId { get; set; }

        public string? SerialNo { get; set; }

        // ── Dropdowns: selected values ──
        public int? SelectedItemTypeId { get; set; }
        public int? SelectedItemBrandId { get; set; }
        public int? SelectedItemModelId { get; set; }
        public int SelectedItemStatusId { get; set; }

        // ── Dropdown lists (rendered into <select> via asp-items) ──
        public List<SelectListItem> ItemTypes { get; set; } = new();
        public List<SelectListItem> ItemBrands { get; set; } = new();
        public List<SelectListItem> ItemModels { get; set; } = new();
        public List<SelectListItem> ItemStatuses { get; set; } = new();

        // ── Cascade maps (serialized to JSON in the View) ──
        //    Populated in the controller from ItemTypeToBrand table.

        /// <summary>ItemType.Id → list of ItemBrand.Id (many-to-many via ItemTypeToBrand)</summary>
        public Dictionary<int, List<int>> TypeToBrandsMap { get; set; } = new();

        /// <summary>ItemBrand.Id → list of ItemType.Id (reverse of above)</summary>
        public Dictionary<int, List<int>> BrandToTypesMap { get; set; } = new();

        /// <summary>ItemBrand.Id → list of {Value=ModelId, Text=ModelName} (one-to-many)</summary>
        public Dictionary<int, List<SelectListItem>> BrandToModelsMap { get; set; } = new();

        // ── Item fields ──
        public string? ItemDescription { get; set; }
        public string? ItemImageUrl { get; set; }
        public IFormFile? ItemImageFile { get; set; }
        public int TotalQuantity { get; set; }
        public int MinimumQuantity { get; set; }
        public string? Remarks { get; set; }
    }
}