using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class DepartmentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อฝ่าย/กลุ่มงาน")]
        public string Name { get; set; } = string.Empty;
    }
}
