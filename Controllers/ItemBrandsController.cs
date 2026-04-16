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
    public class ItemBrandsController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public ItemBrandsController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // GET: ItemBrands
        public async Task<IActionResult> Index(string sortOrder, string searchBox)
        {
            sortOrder ??= "name_asc";
            ViewBag.CurrentSort   = sortOrder;
            ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByName    = sortOrder == "name_asc"    ? "name_desc"    : "name_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";

            var query = _context.ItemBrands.Where(i => !i.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string pattern = $"%{searchBox}%";
                query = query.Where(i => EF.Functions.Like(i.Name, pattern));
            }

            query = sortOrder switch
            {
                "name_asc"     => query.OrderBy(i => i.Name),
                "name_desc"    => query.OrderByDescending(i => i.Name),
                "moddate_asc"  => query.OrderBy(i => i.ModifiedDate),
                "moddate_desc" => query.OrderByDescending(i => i.ModifiedDate),
                _              => query.OrderBy(i => i.Name)
            };

            return View(await query.ToListAsync());
        }

        // GET: ItemBrands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemBrand = await _context.ItemBrands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemBrand == null)
            {
                return NotFound();
            }

            return View(itemBrand);
        }

        // GET: ItemBrands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ItemBrands/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemBrandViewModel ibVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
                return View(ibVM);
            }
            
            string itUser = Utility.GetCurrentUserName();
            ItemBrand itemBrand = new ItemBrand
            {
                Name = ibVM.Name,
                CreatedBy = itUser,
                ModifiedBy = itUser,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsDeleted = false
            };
            _context.ItemBrands.Add(itemBrand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ItemBrands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var itemBrand = await _context.ItemBrands.FindAsync(id);
            if (itemBrand == null) return NotFound();
            var vm = new ItemBrandEditViewModel { Id = itemBrand.Id, Name = itemBrand.Name };
            return View(vm);
        }

        // POST: ItemBrands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemBrandEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) return View(vm);
            var itemBrand = await _context.ItemBrands.FindAsync(id);
            if (itemBrand == null) return NotFound();
            string itUser = Utility.GetCurrentUserName();
            itemBrand.Name = vm.Name;
            itemBrand.ModifiedBy = itUser;
            itemBrand.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ItemBrands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemBrand = await _context.ItemBrands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemBrand == null)
            {
                return NotFound();
            }

            return View(itemBrand);
        }

        // POST: ItemBrands/Delete/5 — soft delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemBrand = await _context.ItemBrands.FindAsync(id);
            if (itemBrand != null)
            {
                string itUser = Utility.GetCurrentUserName();
                itemBrand.IsDeleted = true;
                itemBrand.ModifiedBy = itUser;
                itemBrand.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ItemBrandExists(int id)
        {
            return _context.ItemBrands.Any(e => e.Id == id);
        }
    }
}
