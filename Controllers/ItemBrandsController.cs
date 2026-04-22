using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class ItemBrandsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        public ItemBrandsController(ItLptWarehouseContext context) => _context = context;

        public async Task<IActionResult> Index(string sortOrder, string searchBox)
        {
            sortOrder ??= "name_asc";
            ViewBag.CurrentSort = sortOrder; ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByName    = sortOrder == "name_asc"    ? "name_desc"    : "name_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";
            var q = _context.ItemBrands.Where(i => !i.IsDeleted).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchBox)) { string p = $"%{searchBox}%"; q = q.Where(i => EF.Functions.Like(i.Name, p)); }
            q = sortOrder switch { "name_desc" => q.OrderByDescending(i => i.Name), "moddate_asc" => q.OrderBy(i => i.ModifiedDate), "moddate_desc" => q.OrderByDescending(i => i.ModifiedDate), _ => q.OrderBy(i => i.Name) };
            return View(await q.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var b = await _context.ItemBrands.FirstOrDefaultAsync(m => m.Id == id);
            return b == null ? NotFound() : View(b);
        }

        public IActionResult Create() => View(new ItemBrandViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemBrandViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            string u = User.GetUsernameLocalPart();
            _context.ItemBrands.Add(new ItemBrand { Name = vm.Name, CreatedBy = u, ModifiedBy = u, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, IsDeleted = false });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var b = await _context.ItemBrands.FindAsync(id);
            return b == null ? NotFound() : View(new ItemBrandEditViewModel { Id = b.Id, Name = b.Name });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemBrandEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) return View(vm);
            var b = await _context.ItemBrands.FindAsync(id);
            if (b == null) return NotFound();
            string u = User.GetUsernameLocalPart();
            b.Name = vm.Name; b.ModifiedBy = u; b.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var b = await _context.ItemBrands.Include(x => x.ItemModels).Include(x => x.Items).FirstOrDefaultAsync(m => m.Id == id);
            if (b == null) return NotFound();
            var vm = new ItemBrandDeleteViewModel
            {
                Id = b.Id, Name = b.Name, ModifiedDate = b.ModifiedDate,
                ItemModelCount = b.ItemModels.Count(m => !m.IsDeleted),
                ItemCount      = b.Items.Count(i => !i.IsDeleted)
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var b = await _context.ItemBrands.Include(x => x.ItemModels).Include(x => x.Items).FirstOrDefaultAsync(i => i.Id == id);
            if (b == null) return NotFound();
            if (b.ItemModels.Any(m => !m.IsDeleted) || b.Items.Any(i => !i.IsDeleted)) return RedirectToAction(nameof(Delete), new { id });
            string u = User.GetUsernameLocalPart();
            b.IsDeleted = true; b.ModifiedBy = u; b.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
