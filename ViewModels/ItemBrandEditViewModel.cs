using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class ItemBrandEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อยี่ห้อ")]
        public string Name { get; set; } = string.Empty;
    }
}
