using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KlangIT_V3.Models;

namespace KlangIT_V3.Controllers
{
    public class ItemTypeToBrandsController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public ItemTypeToBrandsController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // GET: ItemTypeToBrands
        public async Task<IActionResult> Index()
        {
            var itLptWarehouseContext = _context.ItemTypeToBrands.Include(i => i.ItemBrand).Include(i => i.ItemType)
                .OrderBy(it => it.ItemType.Name).ThenBy(ib => ib.ItemBrand.Name);
            return View(await itLptWarehouseContext.ToListAsync());
        }

        // GET: ItemTypeToBrands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemTypeToBrand = await _context.ItemTypeToBrands
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemTypeToBrand == null)
            {
                return NotFound();
            }

            return View(itemTypeToBrand);
        }

        // GET: ItemTypeToBrands/Create
        public IActionResult Create()
        {
            //ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands, "Id", "Id");
            //ViewData["ItemTypeId"] = new SelectList(_context.ItemTypes, "Id", "Id");
            ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands.OrderBy(ib => ib.Name), "Id", "Name");
            ViewData["ItemTypeId"] = new SelectList(_context.ItemTypes.OrderBy(it => it.Name), "Id", "Name");
            return View();
        }

        // POST: ItemTypeToBrands/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ItemTypeId,ItemBrandId")] ItemTypeToBrand itemTypeToBrand)
        {
            _context.Add(itemTypeToBrand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        // GET: ItemTypeToBrands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemTypeToBrand = await _context.ItemTypeToBrands.FindAsync(id);
            if (itemTypeToBrand == null)
            {
                return NotFound();
            }
            ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands, "Id", "Id", itemTypeToBrand.ItemBrandId);
            ViewData["ItemTypeId"] = new SelectList(_context.ItemTypes, "Id", "Id", itemTypeToBrand.ItemTypeId);
            return View(itemTypeToBrand);
        }

        // POST: ItemTypeToBrands/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ItemTypeId,ItemBrandId")] ItemTypeToBrand itemTypeToBrand)
        {
            if (id != itemTypeToBrand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(itemTypeToBrand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemTypeToBrandExists(itemTypeToBrand.Id))
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
            ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands, "Id", "Id", itemTypeToBrand.ItemBrandId);
            ViewData["ItemTypeId"] = new SelectList(_context.ItemTypes, "Id", "Id", itemTypeToBrand.ItemTypeId);
            return View(itemTypeToBrand);
        }

        // GET: ItemTypeToBrands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemTypeToBrand = await _context.ItemTypeToBrands
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemTypeToBrand == null)
            {
                return NotFound();
            }

            return View(itemTypeToBrand);
        }

        // POST: ItemTypeToBrands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemTypeToBrand = await _context.ItemTypeToBrands.FindAsync(id);
            if (itemTypeToBrand != null)
            {
                _context.ItemTypeToBrands.Remove(itemTypeToBrand);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemTypeToBrandExists(int id)
        {
            return _context.ItemTypeToBrands.Any(e => e.Id == id);
        }
    }
}
