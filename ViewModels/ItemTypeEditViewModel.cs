using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class ItemTypeEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อประเภทอุปกรณ์")]
        public string Name { get; set; } = string.Empty;
    }
}
