using KlangIT_V3.Models.Enums;
using KlangIT_V3.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class ItemCreateViewModel : IValidatableObject
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

        //public int SelectedItemStatusId { get; set; }
        public ItemStatusEnum? SelectedItemStatus { get; set; }  // nullable to allow "not selected"
        

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
        public bool IsBulk { get; set; }
        public int TotalAmount { get; set; }

        public int MinimumAmount { get; set; }
        public string? Remarks { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinimumAmount >= TotalAmount)
            {
                yield return new ValidationResult(
                    "จำนวนขั้นต่ำต้องน้อยกว่าจำนวนทั้งหมด",
                    new[] { nameof(MinimumAmount) }
                );
            }

            var parts = new[] { AssetId1, AssetId2, AssetId3, AssetId4 };
            int filled = parts.Count(p => !string.IsNullOrWhiteSpace(p));
            if (filled > 0 && filled < 4)
            {
                yield return new ValidationResult(
                    "ต้องกรอกเลขครุภัณฑ์ครบทั้ง 4 ช่อง หรือเว้นว่างทั้งหมด",
                    new[] { nameof(AssetId1) }
                );
            }
        }
    }
}