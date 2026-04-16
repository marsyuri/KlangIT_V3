using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class ItemTypeToBrandEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกประเภทอุปกรณ์")]
        public int SelectedItemTypeId { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกยี่ห้อ")]
        public int SelectedItemBrandId { get; set; }

        public List<SelectListItem> ItemTypes { get; set; } = new();
        public List<SelectListItem> ItemBrands { get; set; } = new();
    }
}
