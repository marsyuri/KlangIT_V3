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
    public class ItemTypesController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public ItemTypesController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // GET: ItemTypes
        public async Task<IActionResult> Index(string sortOrder, string searchBox)
        {
            sortOrder ??= "name_asc";
            ViewBag.CurrentSort   = sortOrder;
            ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByName    = sortOrder == "name_asc"    ? "name_desc"    : "name_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";

            var query = _context.ItemTypes.Where(it => !it.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string pattern = $"%{searchBox}%";
                query = query.Where(it => EF.Functions.Like(it.Name, pattern));
            }

            query = sortOrder switch
            {
                "name_asc"     => query.OrderBy(it => it.Name),
                "name_desc"    => query.OrderByDescending(it => it.Name),
                "moddate_asc"  => query.OrderBy(it => it.ModifiedDate),
                "moddate_desc" => query.OrderByDescending(it => it.ModifiedDate),
                _              => query.OrderBy(it => it.Name)
            };

            return View(await query.ToListAsync());
        }

        // GET: ItemTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemType = await _context.ItemTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemType == null)
            {
                return NotFound();
            }

            return View(itemType);
        }

        // GET: ItemTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ItemTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemTypeViewModel itVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
                return View(itVM);
            }
            string itUser = Utility.GetCurrentUserName();
            ItemType itemType = new ItemType
            {
                Name = itVM.Name,
                CreatedBy = itUser,
                ModifiedBy = itUser,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsDeleted = false
            };
            _context.ItemTypes.Add(itemType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ItemTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var itemType = await _context.ItemTypes.FindAsync(id);
            if (itemType == null) return NotFound();
            var vm = new ItemTypeEditViewModel { Id = itemType.Id, Name = itemType.Name };
            return View(vm);
        }

        // POST: ItemTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemTypeEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) return View(vm);
            var itemType = await _context.ItemTypes.FindAsync(id);
            if (itemType == null) return NotFound();
            string itUser = Utility.GetCurrentUserName();
            itemType.Name = vm.Name;
            itemType.ModifiedBy = itUser;
            itemType.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ItemTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemType = await _context.ItemTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemType == null)
            {
                return NotFound();
            }

            return View(itemType);
        }

        // POST: ItemTypes/Delete/5 — soft delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemType = await _context.ItemTypes.FindAsync(id);
            if (itemType != null)
            {
                string itUser = Utility.GetCurrentUserName();
                itemType.IsDeleted = true;
                itemType.ModifiedBy = itUser;
                itemType.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ItemTypeExists(int id)
        {
            return _context.ItemTypes.Any(e => e.Id == id);
        }
    }
}
