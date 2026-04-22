using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class ItemTypeToBrandsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        public ItemTypeToBrandsController(ItLptWarehouseContext context) => _context = context;

        public async Task<IActionResult> Index(string sortOrder, string searchBox)
        {
            sortOrder ??= "type_asc";
            ViewBag.CurrentSort = sortOrder; ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByType  = sortOrder == "type_asc"  ? "type_desc"  : "type_asc";
            ViewBag.SortByBrand = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";
            var q = _context.ItemTypeToBrands.Include(i => i.ItemBrand).Include(i => i.ItemType).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchBox)) { string p = $"%{searchBox}%"; q = q.Where(i => EF.Functions.Like(i.ItemType.Name, p) || EF.Functions.Like(i.ItemBrand.Name, p)); }
            q = sortOrder switch { "type_desc" => q.OrderByDescending(i => i.ItemType.Name).ThenByDescending(i => i.ItemBrand.Name), "brand_asc" => q.OrderBy(i => i.ItemBrand.Name).ThenBy(i => i.ItemType.Name), "brand_desc" => q.OrderByDescending(i => i.ItemBrand.Name).ThenByDescending(i => i.ItemType.Name), _ => q.OrderBy(i => i.ItemType.Name).ThenBy(i => i.ItemBrand.Name) };
            return View(await q.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var e = await _context.ItemTypeToBrands.Include(i => i.ItemBrand).Include(i => i.ItemType).FirstOrDefaultAsync(m => m.Id == id);
            return e == null ? NotFound() : View(e);
        }

        public IActionResult Create()
        {
            var vm = new ItemTypeToBrandViewModel();
            PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemTypeToBrandViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(vm); return View(vm); }
            string u = User.GetUsernameLocalPart();
            _context.Add(new ItemTypeToBrand { ItemTypeId = vm.SelectedItemTypeId, ItemBrandId = vm.SelectedItemBrandId, CreatedBy = u, ModifiedBy = u, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, IsDeleted = false });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var e = await _context.ItemTypeToBrands.FindAsync(id);
            if (e == null) return NotFound();
            var vm = new ItemTypeToBrandEditViewModel { Id = e.Id, SelectedItemTypeId = e.ItemTypeId, SelectedItemBrandId = e.ItemBrandId };
            PopulateDropdownsEdit(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemTypeToBrandEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) { PopulateDropdownsEdit(vm); return View(vm); }
            var e = await _context.ItemTypeToBrands.FindAsync(id);
            if (e == null) return NotFound();
            string u = User.GetUsernameLocalPart();
            e.ItemTypeId = vm.SelectedItemTypeId; e.ItemBrandId = vm.SelectedItemBrandId; e.ModifiedBy = u; e.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var e = await _context.ItemTypeToBrands.Include(i => i.ItemBrand).Include(i => i.ItemType).FirstOrDefaultAsync(m => m.Id == id);
            if (e == null) return NotFound();
            var vm = new ItemTypeToBrandDeleteViewModel { Id = e.Id, ItemTypeName = e.ItemType?.Name ?? string.Empty, ItemBrandName = e.ItemBrand?.Name ?? string.Empty };
            return View(vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var e = await _context.ItemTypeToBrands.FindAsync(id);
            if (e == null) return NotFound();
            string u = User.GetUsernameLocalPart();
            e.IsDeleted = true; e.ModifiedBy = u; e.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(ItemTypeToBrandViewModel vm)
        {
            vm.ItemTypes = _context.ItemTypes.Where(t => !t.IsDeleted).OrderBy(t => t.Name).Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name, Selected = t.Id == vm.SelectedItemTypeId }).ToList();
            vm.ItemTypes.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกประเภทอุปกรณ์ --" });
            vm.ItemBrands = _context.ItemBrands.Where(b => !b.IsDeleted).OrderBy(b => b.Name).Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name, Selected = b.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }
        private void PopulateDropdownsEdit(ItemTypeToBrandEditViewModel vm)
        {
            vm.ItemTypes = _context.ItemTypes.Where(t => !t.IsDeleted).OrderBy(t => t.Name).Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name, Selected = t.Id == vm.SelectedItemTypeId }).ToList();
            vm.ItemTypes.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกประเภทอุปกรณ์ --" });
            vm.ItemBrands = _context.ItemBrands.Where(b => !b.IsDeleted).OrderBy(b => b.Name).Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name, Selected = b.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }
    }
}
