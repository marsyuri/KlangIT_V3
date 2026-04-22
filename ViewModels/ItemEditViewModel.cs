using KlangIT_V3.Models.Enums;
using KlangIT_V3.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    /// <summary>ViewModel สำหรับ Items/Edit</summary>
    public class ItemEditViewModel : IValidatableObject
    {
        public int Id { get; set; }

        // ── Asset ID ──
        [NumericFixedLengthAttribute(4)]
        public string? AssetId1 { get; set; }

        [NumericFixedLengthAttribute(8)]
        public string? AssetId2 { get; set; }

        [NumericFixedLengthAttribute(8)]
        public string? AssetId3 { get; set; }

        [NumericFixedLengthAttribute(5)]
        public string? AssetId4 { get; set; }

        public string? OtherAssetId { get; set; }
        public string? SerialNo     { get; set; }

        // ── Type / Brand / Model ──
        [Required(ErrorMessage = "กรุณาเลือกประเภทอุปกรณ์")]
        public int SelectedItemTypeId  { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกยี่ห้อ")]
        public int SelectedItemBrandId { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกรุ่น")]
        public int SelectedItemModelId { get; set; }

        public List<SelectListItem> ItemTypes  { get; set; } = new();
        public List<SelectListItem> ItemBrands { get; set; } = new();
        public List<SelectListItem> ItemModels { get; set; } = new();

        // ── Cascade maps (Type ↔ Brand M2M, Brand → Model 1-M) ──
        public Dictionary<int, List<int>> TypeToBrandsMap { get; set; } = new();
        public Dictionary<int, List<int>> BrandToTypesMap { get; set; } = new();
        public Dictionary<int, List<SelectListItem>> BrandToModelsMap { get; set; } = new();

        public string? ItemDescription { get; set; }
        public string? ItemImageUrl    { get; set; }
        public IFormFile? ItemImageFile { get; set; }

        // ── Amounts ──
        [Range(1, int.MaxValue, ErrorMessage = "จำนวนทั้งหมดต้องมากกว่า 0")]
        public int TotalAmount     { get; set; }
        public int AvailableAmount { get; set; }   // readonly — calculated
        public int BorrowedAmount  { get; set; }   // readonly — calculated
        public int DamagedAmount   { get; set; }   // readonly — calculated
        public int DisposedAmount  { get; set; }   // readonly — calculated

        [Range(0, int.MaxValue)]
        public int MinimumAmount { get; set; }

        // ── Status ──
        public ItemStatusEnum SelectedItemStatus { get; set; }
        public List<SelectListItem> ItemStatuses { get; set; } = new();

        public string? Remarks { get; set; }

        // ── Audit (readonly) ──
        public DateTime CreatedDate  { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string   CreatedBy    { get; set; } = string.Empty;
        public string   ModifiedBy   { get; set; } = string.Empty;
        public bool     IsDeleted    { get; set; }

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
                    new[] { "AssetIdBlock" }
                );
            }
        }
    }
}
