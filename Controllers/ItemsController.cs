using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        public async Task<IActionResult> Index(string sortOrder, string filterTypeId, string filterBrandId, string filterModelId, string searchBox)
        {
            sortOrder ??= "asset_asc";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortByAssetId = sortOrder == "asset_asc" ? "asset_desc" : "asset_asc";
            ViewBag.SortByItemType = sortOrder == "type_asc" ? "type_desc" : "type_asc";
            ViewBag.SortByItemBrand = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";
            ViewBag.SortByItemModel = sortOrder == "model_asc" ? "model_desc" : "model_asc";
            ViewBag.SortByAvlNo = sortOrder == "an_asc" ? "an_desc" : "an_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";

            ViewBag.CurrentFilterType = filterTypeId;
            ViewBag.CurrentFilterBrand = filterBrandId;
            ViewBag.CurrentFilterModel = filterModelId;
            ViewBag.CurrentSearch = searchBox;

            // Build cascade maps for 2-way type↔brand and brand→model dropdowns
            var links = await _context.ItemTypeToBrands.ToListAsync();

            var typeToBrandsMap = links
                .GroupBy(l => l.ItemTypeId)
                .ToDictionary(g => g.Key, g => g.Select(l => l.ItemBrandId).ToList());

            var brandToTypesMap = links
                .GroupBy(l => l.ItemBrandId)
                .ToDictionary(g => g.Key, g => g.Select(l => l.ItemTypeId).ToList());

            var brandToModelsMap = await _context.ItemModels
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Name)
                .GroupBy(m => m.ItemBrandId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(m => new { Value = m.Id.ToString(), Text = m.Name }).ToList());

            ViewBag.TypeToBrandsMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(typeToBrandsMap);
            ViewBag.BrandToTypesMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(brandToTypesMap);
            ViewBag.BrandToModelsMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(brandToModelsMap);

            // Populate filter dropdowns
            ViewBag.ItemTypes = await _context.ItemTypes
                .Where(t => !t.IsDeleted).OrderBy(t => t.Name)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name, Selected = t.Id.ToString() == filterTypeId })
                .ToListAsync();

            ViewBag.ItemBrands = await _context.ItemBrands
                .Where(b => !b.IsDeleted).OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name, Selected = b.Id.ToString() == filterBrandId })
                .ToListAsync();

            ViewBag.ItemModels = await _context.ItemModels
                .Where(m => !m.IsDeleted).OrderBy(m => m.Name)
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name, Selected = m.Id.ToString() == filterModelId })
                .ToListAsync();

            var query = _context.Items
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemModel)
                .Include(i => i.ItemType)
                .Where(i => !i.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterTypeId) && int.TryParse(filterTypeId, out int typeId))
                query = query.Where(i => i.ItemTypeId == typeId);

            if (!string.IsNullOrWhiteSpace(filterBrandId) && int.TryParse(filterBrandId, out int brandId))
                query = query.Where(i => i.ItemBrandId == brandId);

            if (!string.IsNullOrWhiteSpace(filterModelId) && int.TryParse(filterModelId, out int modelId))
                query = query.Where(i => i.ItemModelId == modelId);

            if (!string.IsNullOrWhiteSpace(searchBox))
            {
                string pattern = $"%{searchBox}%";
                query = query.Where(i =>
                    EF.Functions.Like(i.AssetId ?? "", pattern) ||
                    EF.Functions.Like(i.SerialNo ?? "", pattern) ||
                    EF.Functions.Like(i.ItemType.Name, pattern) ||
                    EF.Functions.Like(i.ItemBrand.Name, pattern) ||
                    EF.Functions.Like(i.ItemModel.Name, pattern));
            }

            query = sortOrder switch
            {
                "asset_asc" => query.OrderBy(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenBy(s => s.AssetId).ThenBy(s => s.ItemType.Name),
                "asset_desc" => query.OrderByDescending(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenByDescending(s => s.AssetId).ThenByDescending(s => s.ItemType.Name),
                "type_asc" => query.OrderBy(s => string.IsNullOrWhiteSpace(s.ItemType.Name) ? 1 : 0).ThenBy(s => s.ItemType.Name).ThenBy(s => s.AssetId),
                "type_desc" => query.OrderByDescending(s => string.IsNullOrWhiteSpace(s.ItemType.Name) ? 1 : 0).ThenByDescending(s => s.ItemType.Name).ThenByDescending(s => s.AssetId),
                "brand_asc" => query.OrderBy(s => string.IsNullOrWhiteSpace(s.ItemBrand.Name) ? 1 : 0).ThenBy(s => s.ItemBrand.Name).ThenBy(s => s.ItemModel.Name),
                "brand_desc" => query.OrderByDescending(s => string.IsNullOrWhiteSpace(s.ItemBrand.Name) ? 1 : 0).ThenByDescending(s => s.ItemBrand.Name).ThenByDescending(s => s.ItemModel.Name),
                "model_asc" => query.OrderBy(s => string.IsNullOrWhiteSpace(s.ItemModel.Name) ? 1 : 0).ThenBy(s => s.ItemModel.Name).ThenBy(s => s.ItemBrand.Name),
                "model_desc" => query.OrderByDescending(s => string.IsNullOrWhiteSpace(s.ItemModel.Name) ? 1 : 0).ThenByDescending(s => s.ItemModel.Name).ThenByDescending(s => s.ItemBrand.Name),
                "an_asc" => query.OrderBy(s => s.AvailableAmount).ThenBy(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenBy(s => s.AssetId),
                "an_desc" => query.OrderByDescending(s => s.AvailableAmount).ThenByDescending(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenByDescending(s => s.AssetId),
                "moddate_asc" => query.OrderBy(s => s.ModifiedDate),
                "moddate_desc" => query.OrderByDescending(s => s.ModifiedDate),
                _ => query.OrderBy(s => s.AssetId)
            };

            return View(await query.ToListAsync());
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
            ItemDetailsViewModel viewModel = new ItemDetailsViewModel
            {
                Id = item.Id,
                AssetId = item.AssetId,
                SerialNo = item.SerialNo,
                ItemTypeId = item.ItemTypeId,
                ItemType = item.ItemType,
                ItemBrandId = item.ItemBrandId,
                ItemModelId = item.ItemModelId,
                ItemDescription = item.ItemDescription,
                ItemImageUrl = item.ItemImageUrl,
                TotalAmount = item.TotalAmount,
                ActiveAmount = item.ActiveAmount,
                AvailableAmount = item.AvailableAmount,
                BorrowedAmount = item.BorrowedAmount,
                DamagedAmount = item.DamagedAmount,
                DisposedAmount = item.DisposedAmount,
                MinimumAmount = item.MinimumAmount,
                ItemStatus = item.ItemStatus,
                AssetId1 = item.AssetId1,
                AssetId2 = item.AssetId2,
                AssetId3 = item.AssetId3,
                AssetId4 = item.AssetId4,
                OtherAssetId = item.OtherAssetId,
                Remarks = item.Remarks,
                CreatedDate = item.CreatedDate,
                ModifiedDate = item.ModifiedDate,
                CreatedBy = item.CreatedBy,
                ModifiedBy = item.ModifiedBy
            };
            item.BorrowHistories = await _context.BorrowHistories.Include(b => b.RequestDepartment).Include(b => b.RequestSection)
                .Where(b => b.ItemId == item.Id)
                .ToListAsync();
            List<BorrowInItemDetailViewModel> bhDetails = new List<BorrowInItemDetailViewModel>();
            if (item.BorrowHistories != null && item.BorrowHistories.Count > 0)
            {
                var bhOrder = item.BorrowHistories.OrderByDescending(b => b.Id).ToList();
                foreach (var bh in bhOrder)
                {
                    bhDetails.Add(new BorrowInItemDetailViewModel
                    {
                        Id = bh.Id,
                        RequestUser = bh.RequestUser,
                        RequestDepartment = bh.RequestDepartment?.Name ?? string.Empty,
                        RequestSection = bh.RequestSection?.Name ?? string.Empty,
                        BorrowDate = bh.BorrowDate,
                        LatestITStaff = bh.Itstaff,
                        IsReturn = bh.IsReturn
                    });
                }
                viewModel.BHinItemDetails = bhDetails;
            }
            return View(viewModel);
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
            viewModel.SelectedItemStatus = ItemStatusEnum.Available;
            viewModel.TotalAmount = 1;
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
                    Selected = (ItemStatusEnum)e == vm.SelectedItemStatus
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
            string itUser = Utility.GetCurrentUserName();
            Item item = new Item
            {
                IsBulk = itemVM.IsBulk,
                AssetId = assetId,
                AssetId1 = itemVM.AssetId1,
                AssetId2 = itemVM.AssetId2,
                AssetId3 = itemVM.AssetId3,
                AssetId4 = itemVM.AssetId4,
                OtherAssetId = itemVM.OtherAssetId,
                SerialNo = itemVM.SerialNo,
                ItemTypeId = itemVM.SelectedItemTypeId,
                ItemBrandId = itemVM.SelectedItemBrandId,
                ItemModelId = itemVM.SelectedItemModelId,
                ItemDescription = itemVM.ItemDescription,
                ItemImageUrl = imageUrl,
                TotalAmount = itemVM.IsBulk ? itemVM.TotalAmount : 1,
                ActiveAmount = itemVM.TotalAmount,
                AvailableAmount = itemVM.TotalAmount,
                BorrowedAmount = 0,
                DamagedAmount = 0,
                DisposedAmount = 0,
                MinimumAmount = itemVM.MinimumAmount,
                ItemStatus = ItemStatusEnum.Available,
                Remarks = itemVM.Remarks,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CreatedBy = itUser,
                ModifiedBy = itUser,
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
                Remarks = StockLogTypeEnum.Initial.GetDisplayName(),
                RunningNo = runNo,
                LogNo = $"ST{logNoLeftPadded}",
                StockLogType = StockLogTypeEnum.Initial,
                CreatedDate = DateTime.Now,
                CreatedBy = itUser
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
