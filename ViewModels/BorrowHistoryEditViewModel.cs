using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class BorrowHistoryEditViewModel
    {
        public int Id { get; set; }

        [Required] public string RequestUser { get; set; } = string.Empty;

        [Required] public int SelectedDepartmentId { get; set; }
        public int SelectedSectionId { get; set; }

        [DisplayName("ยืมถาวร")] public bool IsPermanentBorrow { get; set; }

        [Required] public DateTime BorrowDate { get; set; } = DateTime.Now;

        [DisplayName("มีวันคืนที่คาดไว้")] public bool HasExpectedReturnDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        [DisplayName("คืนแล้ว")] public bool IsReturn { get; set; }
        public DateTime? ReturnDate { get; set; }

        [Required] public string Itstaff { get; set; } = string.Empty;

        [Required] public int Amount { get; set; }

        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> Sections { get; set; } = new();
    }
}
