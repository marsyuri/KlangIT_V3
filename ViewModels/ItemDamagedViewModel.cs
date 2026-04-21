using KlangIT_V3.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class ItemDamagedViewModel
    {
        public int ItemId { get; set; }

        public string? ItemHeader { get; set; } = string.Empty;

        public string? ItemAssetId { get; set; } = string.Empty;

        public ItemStatusEnum ItemStatus { get; set; }

        public int AvailableAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "จำนวนต้องมากกว่า 0")]
        public int Amount { get; set; } = 1;

        [Required(ErrorMessage = "กรุณาระบุชื่อผู้ดำเนินการ")]
        public string Itstaff { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }
}
