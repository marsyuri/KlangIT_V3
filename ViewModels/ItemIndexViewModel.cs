using KlangIT_V3.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KlangIT_V3.ViewModels
{
    /// <summary>ViewModel สำหรับ Items/Index - รองรับ filter, sort และ cascade dropdowns</summary>
    public class ItemIndexViewModel
    {
        // ── Filter values ──
        public string? FilterTypeId  { get; set; }
        public string? FilterBrandId { get; set; }
        public string? FilterModelId { get; set; }
        public string? SearchBox     { get; set; }

        // ── Sort ──
        public string CurrentSort    { get; set; } = "asset_asc";

        // ── Dropdown lists ──
        public List<SelectListItem> ItemTypes  { get; set; } = new();
        public List<SelectListItem> ItemBrands { get; set; } = new();
        public List<SelectListItem> ItemModels { get; set; } = new();

        // ── Cascade maps (serialized to JSON in View) ──
        public string TypeToBrandsMapJson  { get; set; } = "{}";
        public string BrandToTypesMapJson  { get; set; } = "{}";
        public string BrandToModelsMapJson { get; set; } = "{}";

        // ── Data ──
        public List<ItemRowViewModel> Items { get; set; } = new();
    }

    /// <summary>แถวข้อมูลในตาราง Items/Index</summary>
    public class ItemRowViewModel
    {
        public int    Id             { get; set; }
        public string AssetId        { get; set; } = string.Empty;
        public string ItemTypeName   { get; set; } = string.Empty;
        public string ItemBrandName  { get; set; } = string.Empty;
        public string ItemModelName  { get; set; } = string.Empty;
        public int    AvailableAmount{ get; set; }
        public DateTime ModifiedDate { get; set; }
        public ItemStatusEnum ItemStatus { get; set; }
    }
}
