using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.ViewModels
{
    public class SectionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int SelectedDepartmentId { get; set; }

        public List<SelectListItem> Departments { get; set; } = new();

    }


}
