using KlangIT_V3.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KlangIT_V3.ViewModels
{
    public class ReturnCreateViewModel
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string? ItemHeader { get; set; } = string.Empty;

        public string? ItemAssetId { get; set; } = string.Empty;

        public ItemStatusEnum ItemStatus { get; set; }

        public string RequestUser { get; set; } = null!;

        public int SelectedDepartmentId { get; set; }

        public List<SelectListItem> Departments { get; set; } = new();
        public int SelectedSectionId { get; set; }

        public List<SelectListItem> Sections { get; set; } = new();

        public DateTime BorrowDate { get; set; }

        public int? DurationDays { get; set; }

        public string Itstaff { get; set; } = null!;

        public int ReturnAmount { get; set; }
    }
}
