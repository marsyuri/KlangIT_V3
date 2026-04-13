using Microsoft.AspNetCore.Mvc.Rendering;

namespace KlangIT_V3.ViewModels
{
    public class ItemModelViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int SelectedItemBrandId { get; set; }

        public List<SelectListItem> ItemBrands { get; set; } = new();
    }
}
