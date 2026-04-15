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
    public class ItemModelsController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public ItemModelsController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // GET: ItemModels
        public async Task<IActionResult> Index(string sortOrder)
        {
            var itemModel = _context.ItemModels.Where(s => !s.IsDeleted);
            if (itemModel is null)
            {
                return NotFound();
            }
            if (sortOrder is null)
            {
                sortOrder = "brand_asc";
            }
            ViewBag.SortByName = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.SortByBrand = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";
            itemModel = sortOrder switch
            {
                "name_asc" => itemModel.OrderBy(i => i.Name),
                "name_desc" => itemModel.OrderByDescending(i => i.Name),
                "brand_asc" => itemModel.OrderBy(i => i.ItemBrand.Name).ThenBy(i => i.Name),
                "brand_desc" => itemModel.OrderByDescending(i => i.ItemBrand.Name).ThenByDescending(i => i.Name),
                "moddate_asc" => itemModel.OrderBy(i => i.ModifiedDate),
                "moddate_desc" => itemModel.OrderByDescending(i => i.ModifiedDate),
                _ => itemModel.OrderBy(i => i.ItemBrand.Name)
            };
            return View(await itemModel.ToListAsync());
        }

        // GET: ItemModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemModel = await _context.ItemModels
                .Include(i => i.ItemBrand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemModel == null)
            {
                return NotFound();
            }

            return View(itemModel);
        }

        // GET: ItemModels/Create
        public IActionResult Create()
        {
            var viewModel = new ItemModelViewModel();
            PopulateItemBrands(viewModel);
            return View(viewModel);
        }

        // POST: ItemModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemModelViewModel imVM)
        {
            if (ModelState.IsValid)
            {
                string itUser = Utility.GetCurrentUserName();
                ItemModel im = new ItemModel
                {
                    Name = imVM.Name,
                    ItemBrandId = imVM.SelectedItemBrandId,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CreatedBy = itUser,
                    ModifiedBy = itUser,
                    IsDeleted = false
                };
                _context.ItemModels.Add(im);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(imVM);
        }

        // GET: ItemModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemModel = await _context.ItemModels.FindAsync(id);
            if (itemModel == null)
            {
                return NotFound();
            }
            ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands, "Id", "Id", itemModel.ItemBrandId);
            return View(itemModel);
        }

        // POST: ItemModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ItemBrandId,CreatedDate,ModifiedDate,CreatedBy,ModifiedBy,IsDeleted")] ItemModel itemModel)
        {
            if (id != itemModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(itemModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemModelExists(itemModel.Id))
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
            ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands, "Id", "Id", itemModel.ItemBrandId);
            return View(itemModel);
        }

        // GET: ItemModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemModel = await _context.ItemModels
                .Include(i => i.ItemBrand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemModel == null)
            {
                return NotFound();
            }

            return View(itemModel);
        }

        // POST: ItemModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemModel = await _context.ItemModels.FindAsync(id);
            if (itemModel != null)
            {
                _context.ItemModels.Remove(itemModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemModelExists(int id)
        {
            return _context.ItemModels.Any(e => e.Id == id);
        }

        private void PopulateItemBrands(ItemModelViewModel model)
        {
            model.ItemBrands = _context.ItemBrands
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == model.SelectedItemBrandId
                }).ToList();

            model.ItemBrands.Insert(0,
                new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }
    }
}
