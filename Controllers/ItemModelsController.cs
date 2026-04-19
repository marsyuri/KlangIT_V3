using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class ItemModelsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        public ItemModelsController(ItLptWarehouseContext context) => _context = context;

        public async Task<IActionResult> Index(string sortOrder, string filterBrandId, string searchBox)
        {
            sortOrder ??= "brand_asc";
            ViewBag.CurrentSort      = sortOrder;
            ViewBag.CurrentFilterBrand = filterBrandId;
            ViewBag.CurrentSearch    = searchBox;
            ViewBag.SortByName       = sortOrder == "name_asc"    ? "name_desc"    : "name_asc";
            ViewBag.SortByBrand      = sortOrder == "brand_asc"   ? "brand_desc"   : "brand_asc";
            ViewBag.SortByModDate    = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";

            ViewBag.ItemBrands = await _context.ItemBrands.Where(b => !b.IsDeleted).OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name, Selected = b.Id.ToString() == filterBrandId })
                .ToListAsync();

            var query = _context.ItemModels.Include(i => i.ItemBrand).Where(i => !i.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterBrandId) && int.TryParse(filterBrandId, out int bId))
                query = query.Where(i => i.ItemBrandId == bId);
            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string p = $"%{searchBox}%";
                query = query.Where(i => EF.Functions.Like(i.Name, p) || EF.Functions.Like(i.ItemBrand.Name, p));
            }
            query = sortOrder switch
            {
                "name_asc"    => query.OrderBy(i => i.Name),
                "name_desc"   => query.OrderByDescending(i => i.Name),
                "brand_asc"   => query.OrderBy(i => i.ItemBrand.Name).ThenBy(i => i.Name),
                "brand_desc"  => query.OrderByDescending(i => i.ItemBrand.Name).ThenByDescending(i => i.Name),
                "moddate_asc" => query.OrderBy(i => i.ModifiedDate),
                "moddate_desc"=> query.OrderByDescending(i => i.ModifiedDate),
                _             => query.OrderBy(i => i.ItemBrand.Name)
            };
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var m = await _context.ItemModels.Include(i => i.ItemBrand).FirstOrDefaultAsync(x => x.Id == id);
            return m == null ? NotFound() : View(m);
        }

        public IActionResult Create()
        {
            var vm = new ItemModelViewModel();
            PopulateItemBrands(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemModelViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateItemBrands(vm); return View(vm); }
            string u = Utility.GetCurrentUserName();
            _context.ItemModels.Add(new ItemModel
            {
                Name = vm.Name, ItemBrandId = vm.SelectedItemBrandId,
                CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedBy = u, ModifiedBy = u, IsDeleted = false
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ItemModels.FindAsync(id);
            if (item == null) return NotFound();
            var vm = new ItemModelEditViewModel { Id = item.Id, Name = item.Name, SelectedItemBrandId = item.ItemBrandId };
            PopulateItemBrandsEdit(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemModelEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) { PopulateItemBrandsEdit(vm); return View(vm); }
            var item = await _context.ItemModels.FindAsync(id);
            if (item == null) return NotFound();
            string u = Utility.GetCurrentUserName();
            item.Name = vm.Name; item.ItemBrandId = vm.SelectedItemBrandId;
            item.ModifiedBy = u; item.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ItemModels.Include(i => i.ItemBrand).Include(i => i.Items).FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            var vm = new ItemModelDeleteViewModel
            {
                Id = item.Id, Name = item.Name, BrandName = item.ItemBrand?.Name ?? string.Empty,
                ModifiedDate = item.ModifiedDate, ModifiedBy = item.ModifiedBy,
                ItemCount = item.Items.Count(i => !i.IsDeleted)
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ItemModels.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();
            if (item.Items.Any(i => !i.IsDeleted)) return RedirectToAction(nameof(Delete), new { id });
            string u = Utility.GetCurrentUserName();
            item.IsDeleted = true; item.ModifiedBy = u; item.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void PopulateItemBrands(ItemModelViewModel vm)
        {
            vm.ItemBrands = _context.ItemBrands.Where(d => !d.IsDeleted).OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }
        private void PopulateItemBrandsEdit(ItemModelEditViewModel vm)
        {
            vm.ItemBrands = _context.ItemBrands.Where(d => !d.IsDeleted).OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }
    }
}
