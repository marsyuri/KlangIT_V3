using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KlangIT_V3.Controllers
{
    public class SectionsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SectionsController(ItLptWarehouseContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUser()
        {
            string username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            int assignIndex = username.IndexOf('@');
            if (assignIndex > 0)
            {
                username = username.Substring(0, assignIndex);
            }
            return username;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchCondition, string searchBox)
        {
            sortOrder ??= "dept_asc";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCondition = searchCondition;
            ViewBag.CurrentSearch = searchBox;

            ViewBag.SortByName = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.SortByDept = sortOrder == "dept_asc" ? "dept_desc" : "dept_asc";
            ViewBag.SortByModBy = sortOrder == "modby_asc" ? "modby_desc" : "modby_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";

            // 1. Base query
            var sections = _context.Sections
                .AsNoTracking()
                .Include(s => s.Department)
                .Where(s => !s.IsDeleted);

            // 2. Filter by department
            if (int.TryParse(searchCondition, out int deptFilterId))
                sections = sections.Where(s => s.DepartmentId == deptFilterId);

            // 3. Search
            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string pattern = $"%{searchBox}%";
                sections = sections.Where(s =>
                    EF.Functions.Like(s.Name, pattern) ||
                    EF.Functions.Like(s.Department.Name, pattern) ||
                    EF.Functions.Like(s.CreatedBy, pattern) ||
                    EF.Functions.Like(s.ModifiedBy, pattern));
            }

            // 4. Sort LAST — after all filters are applied
            sections = sortOrder switch
            {
                "name_asc" => sections.OrderBy(s => s.Name),
                "name_desc" => sections.OrderByDescending(s => s.Name),
                "dept_asc" => sections.OrderBy(s => s.Department.Name).ThenBy(s => s.Name),
                "dept_desc" => sections.OrderByDescending(s => s.Department.Name).ThenByDescending(s => s.Name),
                "moddate_asc" => sections.OrderBy(s => s.ModifiedDate),
                "moddate_desc" => sections.OrderByDescending(s => s.ModifiedDate),
                _ => sections.OrderBy(s => s.Department.Name)
            };

            // 5. Dropdown — separate query
            int? selectedDeptId = int.TryParse(searchCondition, out int p) ? p : null;
            ViewBag.SearchConditions = await _context.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == selectedDeptId
                })
                .ToListAsync();

            return View(await sections.ToListAsync());
        }

        // GET: Sections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // GET: Sections/Create
        public IActionResult Create()
        {
            var viewModel = new SectionViewModel();
            PopulateDepartments(viewModel);
            return View(viewModel);
        }

        // POST: Sections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel sectionVM)
        {
            if (ModelState.IsValid)
            {
                Section section = new Section
                {
                    OrderNo = 1,
                    Name = sectionVM.Name,
                    DepartmentId = sectionVM.SelectedDepartmentId,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CreatedBy = GetCurrentUser(),
                    ModifiedBy = GetCurrentUser(),
                    IsDeleted = false
                };
                _context.Sections.Add(section);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sectionVM);
        }

        // GET: Sections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            var viewModel = new SectionViewModel()
            {
                Id = section.Id,
                Name = section.Name,
                SelectedDepartmentId = section.DepartmentId
            };
            PopulateDepartments(viewModel);
            return View(viewModel);
        }

        // POST: Sections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SectionViewModel sectionVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
                return View(sectionVM);
            }
            var section = await _context.Sections.FindAsync(sectionVM.Id);
            if (section == null)
            {
                return NotFound();
            }
            section.Name = sectionVM.Name;
            section.DepartmentId = sectionVM.SelectedDepartmentId;
            section.ModifiedBy = Utility.GetCurrentUserName();
            section.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Sections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // POST: Sections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section != null)
            {
                _context.Sections.Remove(section);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SectionExists(int id)
        {
            return _context.Sections.Any(e => e.Id == id);
        }

        private void PopulateDepartments(SectionViewModel model)
        {
            model.Departments = _context.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == model.SelectedDepartmentId
                }).ToList();

            model.Departments.Insert(0,
                new SelectListItem { Value = "", Text = "-- เลือกฝ่าย/กลุ่มงาน --" });
        }

        private async Task<List<SelectListItem>> GetDepartmentSelectListAsync(int selectedId = 0)
        {
            var list = await _context.Departments
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == selectedId
                })
                .ToListAsync();

            list.Insert(0, new SelectListItem { Value = "", Text = "-- ทั้งหมด --" });
            return list;
        }
    }
}
