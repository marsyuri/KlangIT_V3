using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class SectionEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อหน่วยงาน")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกฝ่าย/กลุ่มงาน")]
        public int SelectedDepartmentId { get; set; }

        public List<SelectListItem> Departments { get; set; } = new();
    }
}
