using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class ItemTypesController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        public ItemTypesController(ItLptWarehouseContext context) => _context = context;

        public async Task<IActionResult> Index(string sortOrder, string searchBox)
        {
            sortOrder ??= "name_asc";
            ViewBag.CurrentSort = sortOrder; ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByName    = sortOrder == "name_asc"    ? "name_desc"    : "name_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";
            var q = _context.ItemTypes.Where(i => !i.IsDeleted).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchBox)) { string p = $"%{searchBox}%"; q = q.Where(i => EF.Functions.Like(i.Name, p)); }
            q = sortOrder switch { "name_desc" => q.OrderByDescending(i => i.Name), "moddate_asc" => q.OrderBy(i => i.ModifiedDate), "moddate_desc" => q.OrderByDescending(i => i.ModifiedDate), _ => q.OrderBy(i => i.Name) };
            return View(await q.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var t = await _context.ItemTypes.FirstOrDefaultAsync(m => m.Id == id);
            return t == null ? NotFound() : View(t);
        }

        public IActionResult Create() => View(new ItemTypeViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemTypeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            string u = User.GetUsernameLocalPart();
            _context.ItemTypes.Add(new ItemType { Name = vm.Name, CreatedBy = u, ModifiedBy = u, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, IsDeleted = false });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var t = await _context.ItemTypes.FindAsync(id);
            return t == null ? NotFound() : View(new ItemTypeEditViewModel { Id = t.Id, Name = t.Name });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemTypeEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) return View(vm);
            var t = await _context.ItemTypes.FindAsync(id);
            if (t == null) return NotFound();
            string u = User.GetUsernameLocalPart();
            t.Name = vm.Name; t.ModifiedBy = u; t.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var t = await _context.ItemTypes.Include(x => x.Items).FirstOrDefaultAsync(m => m.Id == id);
            if (t == null) return NotFound();
            var vm = new ItemTypeDeleteViewModel { Id = t.Id, Name = t.Name, ModifiedDate = t.ModifiedDate, ItemCount = t.Items.Count(i => !i.IsDeleted) };
            return View(vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t = await _context.ItemTypes.Include(x => x.Items).FirstOrDefaultAsync(i => i.Id == id);
            if (t == null) return NotFound();
            if (t.Items.Any(i => !i.IsDeleted)) return RedirectToAction(nameof(Delete), new { id });
            string u = User.GetUsernameLocalPart();
            t.IsDeleted = true; t.ModifiedBy = u; t.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
