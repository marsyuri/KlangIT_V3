using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enum;
using KlangIT_V3.Models.Enums;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace KlangIT_V3.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemsController(ItLptWarehouseContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var items = _context.Items.Include(i => i.ItemBrand).Include(i => i.ItemModel).Include(i => i.ItemType)
                .Where(i => !i.IsDeleted);
            return View(await items.ToListAsync());
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemModel)
                .Include(i => i.ItemType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            else
            {
                item.BorrowHistories = await _context.BorrowHistories
                    .Where(b => b.ItemId == item.Id)
                    .OrderByDescending(b => b.BorrowDate)
                    .ToListAsync();
            }

            return View(item);
        }

        // GET: Items/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new ItemCreateViewModel();
            PopulateItemTypes(viewModel);
            PopulateItemBrands(viewModel);
            PopulateItemModels(viewModel);
            PopulateItemStatuses(viewModel);
            PopulateCascadeMaps(viewModel);
            return View(viewModel);
        }

        // ── Private helpers ────────────────────────────────────────────────────────

        private void PopulateItemTypes(ItemCreateViewModel vm)
        {
            vm.ItemTypes = _context.ItemTypes
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.Name)
                .Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = i.Name,
                    Selected = i.Id == vm.SelectedItemTypeId
                }).ToList();

            vm.ItemTypes.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกประเภทอุปกรณ์ --" });
        }
        private void PopulateItemBrands(ItemCreateViewModel vm)
        {
            vm.ItemBrands = _context.ItemBrands
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.Name)
                .Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = i.Name,
                    Selected = i.Id == vm.SelectedItemBrandId
                }).ToList();

            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }

        private void PopulateItemModels(ItemCreateViewModel vm)
        {
            vm.ItemModels = _context.ItemModels
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.Name)
                .Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = i.Name,
                    Selected = i.Id == vm.SelectedItemModelId
                }).ToList();

            vm.ItemModels.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกรุ่น --" });
        }

        private void PopulateItemStatuses(ItemCreateViewModel vm)
        {
            vm.ItemStatuses = Enum.GetValues(typeof(ItemStatusEnum))
                .Cast<ItemStatusEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.GetDisplayName(),
                    Selected = (int)e == vm.SelectedItemStatusId
                }).ToList();

            vm.ItemStatuses.Insert(0,
                new SelectListItem { Value = "", Text = "-- เลือกสถานะ --" });
        }

        /// <summary>
        /// Builds three cascade maps from the database and puts them on the ViewModel.
        /// All data is fetched ONCE; no AJAX calls needed at runtime.
        /// </summary>
        private void PopulateCascadeMaps(ItemCreateViewModel vm)
        {
            // ── 1. Many-to-many: ItemType ↔ ItemBrand via ItemTypeToBrand ──
            var links = _context.ItemTypeToBrands.ToList();   // small lookup table

            vm.TypeToBrandsMap = links
                .GroupBy(l => l.ItemTypeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(l => l.ItemBrandId).ToList()
                );

            vm.BrandToTypesMap = links
                .GroupBy(l => l.ItemBrandId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(l => l.ItemTypeId).ToList()
                );

            // ── 2. One-to-many: ItemBrand → ItemModels ──
            vm.BrandToModelsMap = _context.ItemModels
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Name)
                .GroupBy(m => m.ItemBrandId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Name
                    }).ToList()
                );
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCreateViewModel itemVM)
        {
            if (!ModelState.IsValid)
            {
                PopulateItemTypes(itemVM);
                PopulateItemBrands(itemVM);
                PopulateItemModels(itemVM);
                PopulateItemStatuses(itemVM);
                PopulateCascadeMaps(itemVM);
                return View(itemVM);
            }

            string assetId = string.Empty;
            if (!string.IsNullOrWhiteSpace(itemVM.AssetId1) && !string.IsNullOrWhiteSpace(itemVM.AssetId2)
                && !string.IsNullOrWhiteSpace(itemVM.AssetId3) && !string.IsNullOrWhiteSpace(itemVM.AssetId4))
            {
                assetId = string.Format("{0}-{1}-{2}-{3}", itemVM.AssetId1, itemVM.AssetId2, itemVM.AssetId3, itemVM.AssetId4);
            }
            else if (!string.IsNullOrWhiteSpace(itemVM.OtherAssetId))
            {
                assetId = itemVM.OtherAssetId;
            }
            else
            {
                assetId = string.Empty;
            }

            DateTime now = DateTime.Now;
            string nowText = now.ToString("yyyyMMddHHmm");
            int itemsCount = await _context.Items.CountAsync();
            int imgRunNo = itemsCount + 1;
            string imgRunNoLeftPadded = imgRunNo.ToString().PadLeft(5, '0');
            string imgFileName = $"IT{nowText}_{imgRunNoLeftPadded}.jpg";
            string? imageUrl = null;
            if (itemVM.ItemImageFile != null && itemVM.ItemImageFile.Length > 0)
            {
                // ตรวจสอบนามสกุลไฟล์
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(itemVM.ItemImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ItemImageFile", "อนุญาตเฉพาะไฟล์รูปภาพ (.jpg, .png, .gif, .webp)");
                    return View(itemVM);
                }
                // ตรวจสอบขนาดไฟล์ (จำกัด 2MB)
                if (itemVM.ItemImageFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ItemImageFile", "ขนาดไฟล์ต้องไม่เกิน 2MB");
                    return View(itemVM);
                }
                // สร้างชื่อไฟล์ unique เพื่อป้องกันชนกัน
                var uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items");
                // สร้างโฟลเดอร์ถ้ายังไม่มี
                Directory.CreateDirectory(uploadDir);
                var filePath = Path.Combine(uploadDir, imgFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await itemVM.ItemImageFile.CopyToAsync(stream);
                }
                // เก็บ path สำหรับแสดงผลใน View
                imageUrl = $"/uploads/items/{imgFileName}";
            }

            Item item = new Item
            {
                AssetId = assetId,
                SerialNo = itemVM.SerialNo,
                ItemTypeId = itemVM.SelectedItemTypeId,
                ItemBrandId = itemVM.SelectedItemBrandId,
                ItemModelId = itemVM.SelectedItemModelId,
                ItemDescription = itemVM.ItemDescription,
                ItemImageUrl = imageUrl,
                TotalAmount = itemVM.TotalAmount,
                ActiveAmount = itemVM.TotalAmount,
                AvailableAmount = itemVM.TotalAmount,
                BorrowedAmount = 0,
                DamagedAmount = 0,
                MinimumAmount = itemVM.MinimumAmount,
                ItemStatusId = itemVM.SelectedItemStatusId,
                Remarks = itemVM.Remarks,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CreatedBy = Utility.GetCurrentUserName(),
                ModifiedBy = Utility.GetCurrentUserName(),
                IsDeleted = false
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            int runNo = 1;
            string logNoLeftPadded = runNo.ToString().PadLeft(5, '0');
            StockLog stockLog = new StockLog
            {
                ItemId = item.Id,
                OldStock = 0,
                NumberOfChange = item.TotalAmount,
                NewStock = item.TotalAmount,
                Remarks = "สร้างข้อมูล Item ใหม่",
                RunningNo = runNo,
                LogNo = $"st{logNoLeftPadded}",
                StockLogTypeId = (int)StockTypeEnum.Initial,
                CreatedDate = DateTime.Now,
                CreatedBy = Utility.GetCurrentUserName()
            };
            _context.StockLogs.Add(stockLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands, "Id", "Id", item.ItemBrandId);
            ViewData["ItemModelId"] = new SelectList(_context.ItemModels, "Id", "Id", item.ItemModelId);
            //ViewData["ItemStatusId"] = new SelectList(_context.ItemStatuses, "Id", "Id", item.ItemStatusId);
            ViewData["ItemTypeId"] = new SelectList(_context.ItemTypes, "Id", "Id", item.ItemTypeId);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssetId,SerialNo,ItemTypeId,ItemBrandId,ItemModelId,ItemDescription,ItemImageUrl,TotalAmount,AvailableAmount,BorrowedAmount,MinimumAmount,ItemStatusId,AssetId1,AssetId2,AssetId3,AssetId4,Remarks,CreatedDate,ModifiedDate,CreatedBy,ModifiedBy,IsDeleted")] Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
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
            ViewData["ItemBrandId"] = new SelectList(_context.ItemBrands, "Id", "Id", item.ItemBrandId);
            ViewData["ItemModelId"] = new SelectList(_context.ItemModels, "Id", "Id", item.ItemModelId);
            //ViewData["ItemStatusId"] = new SelectList(_context.ItemStatuses, "Id", "Id", item.ItemStatusId);
            ViewData["ItemTypeId"] = new SelectList(_context.ItemTypes, "Id", "Id", item.ItemTypeId);
            return View(item);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemModel)
                .Include(i => i.ItemType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ItemStates()
        {
            return View();
        }
    }
}
