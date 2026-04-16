using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class ItemModelEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อรุ่น")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกยี่ห้อ")]
        public int SelectedItemBrandId { get; set; }

        public List<SelectListItem> ItemBrands { get; set; } = new();
    }
}
