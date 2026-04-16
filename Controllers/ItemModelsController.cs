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
        public async Task<IActionResult> Index(string sortOrder, string filterBrandId, string searchBox)
        {
            sortOrder ??= "brand_asc";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilterBrand = filterBrandId;
            ViewBag.CurrentSearch = searchBox;

            ViewBag.SortByName = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.SortByBrand = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";

            // Populate brand dropdown
            ViewBag.ItemBrands = await _context.ItemBrands
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name,
                    Selected = b.Id.ToString() == filterBrandId
                }).ToListAsync();

            var query = _context.ItemModels
                .Include(i => i.ItemBrand)
                .Where(i => !i.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterBrandId) && int.TryParse(filterBrandId, out int brandId))
                query = query.Where(i => i.ItemBrandId == brandId);

            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string pattern = $"%{searchBox}%";
                query = query.Where(i =>
                    EF.Functions.Like(i.Name, pattern) ||
                    EF.Functions.Like(i.ItemBrand.Name, pattern));
            }

            query = sortOrder switch
            {
                "name_asc" => query.OrderBy(i => i.Name),
                "name_desc" => query.OrderByDescending(i => i.Name),
                "brand_asc" => query.OrderBy(i => i.ItemBrand.Name).ThenBy(i => i.Name),
                "brand_desc" => query.OrderByDescending(i => i.ItemBrand.Name).ThenByDescending(i => i.Name),
                "moddate_asc" => query.OrderBy(i => i.ModifiedDate),
                "moddate_desc" => query.OrderByDescending(i => i.ModifiedDate),
                _ => query.OrderBy(i => i.ItemBrand.Name)
            };

            return View(await query.ToListAsync());
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
            if (id == null) return NotFound();
            var itemModel = await _context.ItemModels.FindAsync(id);
            if (itemModel == null) return NotFound();
            var vm = new ItemModelEditViewModel { Id = itemModel.Id, Name = itemModel.Name, SelectedItemBrandId = itemModel.ItemBrandId };
            PopulateItemBrandsEdit(vm);
            return View(vm);
        }

        // POST: ItemModels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemModelEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) { PopulateItemBrandsEdit(vm); return View(vm); }
            var itemModel = await _context.ItemModels.FindAsync(id);
            if (itemModel == null) return NotFound();
            string itUser = Utility.GetCurrentUserName();
            itemModel.Name = vm.Name;
            itemModel.ItemBrandId = vm.SelectedItemBrandId;
            itemModel.ModifiedBy = itUser;
            itemModel.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void PopulateItemBrandsEdit(ItemModelEditViewModel vm)
        {
            vm.ItemBrands = _context.ItemBrands
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == vm.SelectedItemBrandId
                }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
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

        // POST: ItemModels/Delete/5 — soft delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemModel = await _context.ItemModels.FindAsync(id);
            if (itemModel != null)
            {
                string itUser = Utility.GetCurrentUserName();
                itemModel.IsDeleted = true;
                itemModel.ModifiedBy = itUser;
                itemModel.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
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
