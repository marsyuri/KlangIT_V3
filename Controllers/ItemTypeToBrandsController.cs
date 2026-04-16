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
    public class ItemTypeToBrandsController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public ItemTypeToBrandsController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // GET: ItemTypeToBrands
        public async Task<IActionResult> Index(string sortOrder, string searchBox)
        {
            sortOrder ??= "type_asc";
            ViewBag.CurrentSort   = sortOrder;
            ViewBag.CurrentSearch = searchBox;
            ViewBag.SortByType    = sortOrder == "type_asc"  ? "type_desc"  : "type_asc";
            ViewBag.SortByBrand   = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";

            var query = _context.ItemTypeToBrands
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string pattern = $"%{searchBox}%";
                query = query.Where(i =>
                    EF.Functions.Like(i.ItemType.Name, pattern) ||
                    EF.Functions.Like(i.ItemBrand.Name, pattern));
            }

            query = sortOrder switch
            {
                "type_asc"   => query.OrderBy(i => i.ItemType.Name).ThenBy(i => i.ItemBrand.Name),
                "type_desc"  => query.OrderByDescending(i => i.ItemType.Name).ThenByDescending(i => i.ItemBrand.Name),
                "brand_asc"  => query.OrderBy(i => i.ItemBrand.Name).ThenBy(i => i.ItemType.Name),
                "brand_desc" => query.OrderByDescending(i => i.ItemBrand.Name).ThenByDescending(i => i.ItemType.Name),
                _            => query.OrderBy(i => i.ItemType.Name).ThenBy(i => i.ItemBrand.Name)
            };

            return View(await query.ToListAsync());
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
            var vm = new ItemTypeToBrandViewModel();
            PopulateDropdowns(vm);
            return View(vm);
        }

        // POST: ItemTypeToBrands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemTypeToBrandViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(vm); return View(vm); }
            string itUser = Utility.GetCurrentUserName();
            var entity = new ItemTypeToBrand
            {
                ItemTypeId = vm.SelectedItemTypeId,
                ItemBrandId = vm.SelectedItemBrandId,
                CreatedBy = itUser,
                ModifiedBy = itUser,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsDeleted = false
            };
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ItemTypeToBrands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var entity = await _context.ItemTypeToBrands.FindAsync(id);
            if (entity == null) return NotFound();
            var vm = new ItemTypeToBrandEditViewModel { Id = entity.Id, SelectedItemTypeId = entity.ItemTypeId, SelectedItemBrandId = entity.ItemBrandId };
            PopulateDropdownsEdit(vm);
            return View(vm);
        }

        // POST: ItemTypeToBrands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemTypeToBrandEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            if (!ModelState.IsValid) { PopulateDropdownsEdit(vm); return View(vm); }
            var entity = await _context.ItemTypeToBrands.FindAsync(id);
            if (entity == null) return NotFound();
            string itUser = Utility.GetCurrentUserName();
            entity.ItemTypeId = vm.SelectedItemTypeId;
            entity.ItemBrandId = vm.SelectedItemBrandId;
            entity.ModifiedBy = itUser;
            entity.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        // POST: ItemTypeToBrands/Delete/5 — soft delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.ItemTypeToBrands.FindAsync(id);
            if (entity != null)
            {
                string itUser = Utility.GetCurrentUserName();
                entity.IsDeleted = true;
                entity.ModifiedBy = itUser;
                entity.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ItemTypeToBrandExists(int id)
        {
            return _context.ItemTypeToBrands.Any(e => e.Id == id);
        }
    

        private void PopulateDropdowns(ItemTypeToBrandViewModel vm)
        {
            vm.ItemTypes = _context.ItemTypes.Where(t => !t.IsDeleted).OrderBy(t => t.Name)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name, Selected = t.Id == vm.SelectedItemTypeId }).ToList();
            vm.ItemTypes.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกประเภทอุปกรณ์ --" });
            vm.ItemBrands = _context.ItemBrands.Where(b => !b.IsDeleted).OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name, Selected = b.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }

        private void PopulateDropdownsEdit(ItemTypeToBrandEditViewModel vm)
        {
            vm.ItemTypes = _context.ItemTypes.Where(t => !t.IsDeleted).OrderBy(t => t.Name)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name, Selected = t.Id == vm.SelectedItemTypeId }).ToList();
            vm.ItemTypes.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกประเภทอุปกรณ์ --" });
            vm.ItemBrands = _context.ItemBrands.Where(b => !b.IsDeleted).OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name, Selected = b.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }
    }
}
