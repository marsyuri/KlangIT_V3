using KlangIT_V3.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class BorrowCreateViewModel
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string? ItemHeader { get; set; } = string.Empty;

        public string? ItemAssetId { get; set; } = string.Empty;

        public ItemStatusEnum ItemStatus { get; set; }

        public string RequestUser { get; set; } = null!;

        public int SelectedSectionId { get; set; }

        public List<SelectListItem> Sections { get; set; } = new();


        public int SelectedDepartmentId { get; set; }

        public List<SelectListItem> Departments { get; set; } = new();

        public bool IsPermanentBorrow { get; set; }

        public DateTime BorrowDate { get; set; }

        public bool HasExpectedReturnDate { get; set; }

        public DateTime? ExpectedReturnDate { get; set; }

        public bool IsReturn { get; set; }

        public DateTime? ReturnDate { get; set; }
        
        public int? DurationDays { get; set; }

        public string Itstaff { get; set; } = null!;

        public int Amount { get; set; }
    }
}
