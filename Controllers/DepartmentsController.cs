using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KlangIT_V3.Models;
using KlangIT_V3.ViewModels;
using KlangIT_V3.Helpers;

namespace KlangIT_V3.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public DepartmentsController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> All_Index()
        {
            return View(await _context.Departments.OrderBy(d => d.Name).ToListAsync());
        }

        // GET: Departments
        [HttpGet]
        public async Task<IActionResult> Index(string searchBox)
        {
            ViewBag.CurrentSearch = searchBox;

            var query = _context.Departments.Where(d => !d.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string pattern = $"%{searchBox}%";
                query = query.Where(d => EF.Functions.Like(d.Name, pattern));
            }

            return View(await query.OrderBy(d => d.Name).ToListAsync());
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel deptVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
                return View(deptVM);
            }
            
            string itUser = Utility.GetCurrentUserName();
            Department department = new Department
            {
                Name = deptVM.Name,
                CreatedBy = itUser,
                ModifiedBy = itUser,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsDeleted = false
            };
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            DepartmentViewModel deptVM = new DepartmentViewModel
            {
                Name = department.Name
            };
            return View(deptVM);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentViewModel deptVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
                return View(deptVM);
            }
            var department = await _context.Departments.FindAsync(deptVM.Id);
            if (department == null)
            {
                return NotFound();
            }
            string itUser = Utility.GetCurrentUserName();
            department.Name = deptVM.Name;
            department.ModifiedBy = itUser;
            department.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            string itUser = Utility.GetCurrentUserName();
            if (department != null)
            {
                department.IsDeleted = true;
                department.ModifiedBy = itUser;
                department.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
