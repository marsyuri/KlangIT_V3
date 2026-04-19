using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class SectionsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        public SectionsController(ItLptWarehouseContext context) => _context = context;

        public async Task<IActionResult> Index(string sortOrder, string searchCondition, string searchBox)
        {
            sortOrder ??= "dept_asc";
            ViewBag.CurrentSort = sortOrder; ViewBag.CurrentCondition = searchCondition; ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByName    = sortOrder == "name_asc"    ? "name_desc"    : "name_asc";
            ViewBag.SortByDept    = sortOrder == "dept_asc"    ? "dept_desc"    : "dept_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";

            var q = _context.Sections.AsNoTracking().Include(s => s.Department).Where(s => !s.IsDeleted);
            if (int.TryParse(searchCondition, out int deptId)) q = q.Where(s => s.DepartmentId == deptId);
            if (!string.IsNullOrWhiteSpace(searchBox)) { string p = $"%{searchBox}%"; q = q.Where(s => EF.Functions.Like(s.Name, p) || EF.Functions.Like(s.Department.Name, p)); }
            q = sortOrder switch { "name_asc" => q.OrderBy(s => s.Name), "name_desc" => q.OrderByDescending(s => s.Name), "dept_desc" => q.OrderByDescending(s => s.Department.Name).ThenByDescending(s => s.Name), "moddate_asc" => q.OrderBy(s => s.ModifiedDate), "moddate_desc" => q.OrderByDescending(s => s.ModifiedDate), _ => q.OrderBy(s => s.Department.Name).ThenBy(s => s.Name) };

            int? sel = int.TryParse(searchCondition, out int p2) ? p2 : null;
            ViewBag.SearchConditions = await _context.Departments.Where(d => !d.IsDeleted).OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == sel }).ToListAsync();

            return View(await q.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var s = await _context.Sections.Include(x => x.Department).FirstOrDefaultAsync(m => m.Id == id);
            return s == null ? NotFound() : View(s);
        }

        public IActionResult Create()
        {
            var vm = new SectionViewModel();
            PopulateDepartments(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDepartments(vm); return View(vm); }
            string u = Utility.GetCurrentUserName();
            _context.Sections.Add(new Section { OrderNo = 1, Name = vm.Name, DepartmentId = vm.SelectedDepartmentId, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedBy = u, ModifiedBy = u, IsDeleted = false });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var s = await _context.Sections.FindAsync(id);
            if (s == null) return NotFound();
            var vm = new SectionEditViewModel { Id = s.Id, Name = s.Name, SelectedDepartmentId = s.DepartmentId };
            PopulateDepartmentsEdit(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SectionEditViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDepartmentsEdit(vm); return View(vm); }
            var s = await _context.Sections.FindAsync(vm.Id);
            if (s == null) return NotFound();
            string u = Utility.GetCurrentUserName();
            s.Name = vm.Name; s.DepartmentId = vm.SelectedDepartmentId; s.ModifiedBy = u; s.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var s = await _context.Sections.Include(x => x.Department).Include(x => x.BorrowHistories).FirstOrDefaultAsync(m => m.Id == id);
            if (s == null) return NotFound();
            var vm = new SectionDeleteViewModel
            {
                Id = s.Id, Name = s.Name, DepartmentName = s.Department?.Name ?? string.Empty,
                BorrowHistoryCount = s.BorrowHistories.Count(b => !b.IsDeleted)
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var s = await _context.Sections.FindAsync(id);
            if (s != null) { _context.Sections.Remove(s); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDepartments(SectionViewModel vm)
        {
            vm.Departments = _context.Departments.Where(d => !d.IsDeleted).OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == vm.SelectedDepartmentId }).ToList();
            vm.Departments.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกฝ่าย/กลุ่มงาน --" });
        }
        private void PopulateDepartmentsEdit(SectionEditViewModel vm)
        {
            vm.Departments = _context.Departments.Where(d => !d.IsDeleted).OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == vm.SelectedDepartmentId }).ToList();
            vm.Departments.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกฝ่าย/กลุ่มงาน --" });
        }
    }
}
