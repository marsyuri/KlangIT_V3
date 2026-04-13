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
        public async Task<IActionResult> Index()
        {
            return View(await _context.ItemBrands.OrderBy(i => i.Name).ToListAsync());
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
            ItemBrand itemBrand = new ItemBrand
            {
                Name = ibVM.Name,
                CreatedBy = Utility.GetCurrentUserName(),
                ModifiedBy = Utility.GetCurrentUserName(),
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
            if (id == null)
            {
                return NotFound();
            }

            var itemBrand = await _context.ItemBrands.FindAsync(id);
            if (itemBrand == null)
            {
                return NotFound();
            }
            return View(itemBrand);
        }

        // POST: ItemBrands/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedDate,ModifiedDate,CreatedBy,ModifiedBy,IsDeleted")] ItemBrand itemBrand)
        {
            if (id != itemBrand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(itemBrand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemBrandExists(itemBrand.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(itemBrand);
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

        // POST: ItemBrands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemBrand = await _context.ItemBrands.FindAsync(id);
            if (itemBrand != null)
            {
                _context.ItemBrands.Remove(itemBrand);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemBrandExists(int id)
        {
            return _context.ItemBrands.Any(e => e.Id == id);
        }
    }
}
