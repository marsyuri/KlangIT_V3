using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        public DepartmentsController(ItLptWarehouseContext context) => _context = context;

        public async Task<IActionResult> All_Index()
            => View(await _context.Departments.OrderBy(d => d.Name).ToListAsync());

        public async Task<IActionResult> Index(string sortOrder, string searchBox)
        {
            sortOrder ??= "name_asc";
            ViewBag.CurrentSort = sortOrder; ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByName    = sortOrder == "name_asc"    ? "name_desc"    : "name_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";
            var q = _context.Departments.Where(d => !d.IsDeleted).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchBox)) { string p = $"%{searchBox}%"; q = q.Where(d => EF.Functions.Like(d.Name, p)); }
            q = sortOrder switch { "name_desc" => q.OrderByDescending(d => d.Name), "moddate_asc" => q.OrderBy(d => d.ModifiedDate), "moddate_desc" => q.OrderByDescending(d => d.ModifiedDate), _ => q.OrderBy(d => d.Name) };
            return View(await q.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var d = await _context.Departments.FirstOrDefaultAsync(m => m.Id == id);
            return d == null ? NotFound() : View(d);
        }

        public IActionResult Create() => View(new DepartmentViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            string u = Utility.GetCurrentUserName();
            _context.Departments.Add(new Department { Name = vm.Name, CreatedBy = u, ModifiedBy = u, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, IsDeleted = false });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var d = await _context.Departments.FindAsync(id);
            return d == null ? NotFound() : View(new DepartmentEditViewModel { Id = d.Id, Name = d.Name });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var d = await _context.Departments.FindAsync(vm.Id);
            if (d == null) return NotFound();
            string u = Utility.GetCurrentUserName();
            d.Name = vm.Name; d.ModifiedBy = u; d.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var d = await _context.Departments.Include(x => x.Sections).Include(x => x.BorrowHistories).FirstOrDefaultAsync(m => m.Id == id);
            if (d == null) return NotFound();
            var vm = new DepartmentDeleteViewModel
            {
                Id = d.Id, Name = d.Name,
                SectionCount       = d.Sections.Count(s => !s.IsDeleted),
                BorrowHistoryCount = d.BorrowHistories.Count(b => !b.IsDeleted)
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var d = await _context.Departments.FindAsync(id);
            if (d == null) return NotFound();
            string u = Utility.GetCurrentUserName();
            d.IsDeleted = true; d.ModifiedBy = u; d.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
