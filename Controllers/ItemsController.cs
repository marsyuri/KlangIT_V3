using KlangIT_V3.Data;
using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ItemsController(
            ItLptWarehouseContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // ── GET: Items ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string sortOrder, string filterTypeId, string filterBrandId, string filterModelId, string searchBox)
        {
            sortOrder ??= "asset_asc";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortByAssetId = sortOrder == "asset_asc" ? "asset_desc" : "asset_asc";
            ViewBag.SortByItemType = sortOrder == "type_asc" ? "type_desc" : "type_asc";
            ViewBag.SortByItemBrand = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";
            ViewBag.SortByItemModel = sortOrder == "model_asc" ? "model_desc" : "model_asc";
            ViewBag.SortByAvlNo = sortOrder == "avl_asc" ? "avl_desc" : "avl_asc";
            ViewBag.SortByModDate = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";
            ViewBag.CurrentSearch = searchBox;

            // Cascade maps for dropdowns
            var links = await _context.ItemTypeToBrands.ToListAsync();
            var typeToBrandsMap = links.GroupBy(l => l.ItemTypeId).ToDictionary(g => g.Key, g => g.Select(l => l.ItemBrandId).ToList());
            var brandToTypesMap = links.GroupBy(l => l.ItemBrandId).ToDictionary(g => g.Key, g => g.Select(l => l.ItemTypeId).ToList());
            var brandToModelsMap = await _context.ItemModels
                .Where(m => !m.IsDeleted).OrderBy(m => m.Name)
                .GroupBy(m => m.ItemBrandId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(m => new { Value = m.Id.ToString(), Text = m.Name }).ToList());

            ViewBag.TypeToBrandsMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(typeToBrandsMap);
            ViewBag.BrandToTypesMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(brandToTypesMap);
            ViewBag.BrandToModelsMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(brandToModelsMap);

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
                .Include(i => i.ItemBrand).Include(i => i.ItemModel).Include(i => i.ItemType)
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
                    EF.Functions.Like(i.ItemModel!.Name, pattern));
            }

            query = sortOrder switch
            {
                "asset_asc" => query.OrderBy(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenBy(s => s.AssetId),
                "asset_desc" => query.OrderByDescending(s => string.IsNullOrWhiteSpace(s.AssetId) ? 1 : 0).ThenByDescending(s => s.AssetId),
                "type_asc" => query.OrderBy(s => s.ItemType.Name).ThenBy(s => s.AssetId),
                "type_desc" => query.OrderByDescending(s => s.ItemType.Name).ThenByDescending(s => s.AssetId),
                "brand_asc" => query.OrderBy(s => s.ItemBrand.Name).ThenBy(s => s.ItemModel!.Name),
                "brand_desc" => query.OrderByDescending(s => s.ItemBrand.Name).ThenByDescending(s => s.ItemModel!.Name),
                "model_asc" => query.OrderBy(s => s.ItemModel!.Name).ThenBy(s => s.ItemBrand.Name),
                "model_desc" => query.OrderByDescending(s => s.ItemModel!.Name).ThenByDescending(s => s.ItemBrand.Name),
                "avl_asc" => query.OrderBy(s => s.AvailableAmount),
                "avl_desc" => query.OrderByDescending(s => s.AvailableAmount),
                "moddate_asc" => query.OrderBy(s => s.ModifiedDate),
                "moddate_desc" => query.OrderByDescending(s => s.ModifiedDate),
                _ => query.OrderBy(s => s.AssetId)
            };

            return View(await query.ToListAsync());
        }

        // ── GET: Items/Details/5 ──────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemBrand).Include(i => i.ItemModel).Include(i => i.ItemType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();

            item.BorrowHistories = await _context.BorrowHistories
                .Include(b => b.BorrowerDepartment).Include(b => b.BorrowerSection)
                .Where(b => b.ItemId == item.Id && !b.IsDeleted)
                .ToListAsync();

            var stockLogs = await _context.StockLogs
                .Where(sl => sl.ItemId == item.Id)
                .OrderByDescending(sl => sl.CreatedDate)
                .ThenByDescending(sl => sl.Id)
                .ToListAsync();

            var vm = new ItemDetailsViewModel
            {
                Id = item.Id,
                AssetId = item.AssetId,
                SerialNo = item.SerialNo,
                ItemTypeId = item.ItemTypeId,
                ItemType = item.ItemType,
                ItemBrandId = item.ItemBrandId,
                ItemBrand = item.ItemBrand,
                ItemModelId = item.ItemModelId,
                ItemModel = item.ItemModel,
                ItemDescription = item.ItemDescription,
                ItemImageUrl = item.ItemImageUrl,
                TotalAmount = item.TotalAmount,
                ActiveAmount = item.ActiveAmount,
                AvailableAmount = item.AvailableAmount,
                BorrowedAmount = item.BorrowedAmount,
                DamagedAmount = item.DamagedAmount,
                DisposedAmount = item.DisposedAmount,
                MinimumAmount = item.MinimumAmount,
                ItemStatus = (ItemStatusEnum)item.ItemStatus,
                AssetId1 = item.AssetId1,
                AssetId2 = item.AssetId2,
                AssetId3 = item.AssetId3,
                AssetId4 = item.AssetId4,
                OtherAssetId = item.OtherAssetId,
                Remarks = item.Remarks,
                CreatedDate = item.CreatedDate,
                ModifiedDate = item.ModifiedDate,
                CreatedBy = item.CreatedBy,
                ModifiedBy = item.ModifiedBy,
                BHinItemDetails = item.BorrowHistories.OrderByDescending(b => b.Id).Select(bh => new BorrowInItemDetailViewModel
                {
                    Id = bh.Id,
                    RequestUser = bh.BorrowerUser,
                    RequestDepartment = bh.BorrowerDepartment?.Name ?? string.Empty,
                    RequestSection = bh.BorrowerSection?.Name ?? string.Empty,
                    BorrowDate = bh.BorrowDate,
                    LatestITStaff = bh.ReturnDate.HasValue ? bh.ReturnItname : bh.BorrowItname,
                    IsReturn = bh.ReturnDate.HasValue
                }).ToList(),
                StockTimeline = stockLogs.Select(sl => new StockLogTimelineViewModel
                {
                    Id             = sl.Id,
                    CreatedDate    = sl.CreatedDate,
                    LogType        = (StockLogTypeEnum)sl.LogType,
                    DeltaAvailable = sl.DeltaAvailable,
                    DeltaBorrowed  = sl.DeltaBorrowed,
                    DeltaDamaged   = sl.DeltaDamaged,
                    DeltaDisposed  = sl.DeltaDisposed,
                    DeltaTotal     = sl.DeltaTotal,
                    AvailableAfter = sl.AvailableAfter,
                    BorrowedAfter  = sl.BorrowedAfter,
                    DamagedAfter   = sl.DamagedAfter,
                    DisposedAfter  = sl.DisposedAfter,
                    TotalAfter     = sl.TotalAfter,
                    ReferenceNo    = sl.ReferenceNo,
                    Remarks        = sl.Remarks,
                    CreatedBy      = sl.CreatedBy
                }).ToList()
            };

            return View(vm);
        }

        // ── GET: Items/Create ─────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ItemCreateViewModel();
            PopulateItemTypes(vm);
            PopulateItemBrands(vm);
            PopulateItemModels(vm);
            PopulateItemStatuses(vm);
            PopulateCascadeMaps(vm);
            vm.TotalAmount = 1;
            vm.MinimumAmount= 0;
            return View(vm);
        }

        // ── POST: Items/Create ────────────────────────────────────────────────────
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

            string assetId = BuildAssetId(itemVM);
            string? imageUrl = await SaveImageAsync(itemVM);

            string itUser = User.GetUsernameLocalPart();
            int initialAmount = itemVM.IsBulk ? itemVM.TotalAmount : 1;

            var item = new Item
            {
                IsBulk = itemVM.IsBulk,
                AssetId = assetId,
                AssetId1 = itemVM.AssetId1,
                AssetId2 = itemVM.AssetId2,
                AssetId3 = itemVM.AssetId3,
                AssetId4 = itemVM.AssetId4,
                OtherAssetId = itemVM.OtherAssetId,
                SerialNo = itemVM.SerialNo,
                ItemTypeId = itemVM.SelectedItemTypeId ?? 0,
                ItemBrandId = itemVM.SelectedItemBrandId ?? 0,
                ItemModelId = itemVM.SelectedItemModelId,
                ItemDescription = itemVM.ItemDescription,
                ItemImageUrl = imageUrl,
                TotalAmount = 0,
                ActiveAmount = 0,
                AvailableAmount = 0,
                BorrowedAmount = 0,
                DamagedAmount = 0,
                DisposedAmount = 0,
                MinimumAmount = itemVM.MinimumAmount,
                ItemStatus = (int)(itemVM.SelectedItemStatus ?? ItemStatusEnum.Available),
                Remarks = itemVM.Remarks,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CreatedBy = itUser,
                ModifiedBy = itUser,
                IsDeleted = false
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            await StockHelper.ApplyStockChangeAsync(
                _context,
                item.Id,
                item.ItemStatus,
                deltaAvailable: initialAmount,
                deltaBorrowed: 0,
                deltaDamaged: 0,
                deltaDisposed: 0,
                createdBy: itUser,
                remarks: "รับเข้าเริ่มต้น (Create Item)");

            return RedirectToAction(nameof(Index));
        }

        // ── GET: Items/Edit/5 ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();

            var vm = new ItemEditViewModel
            {
                Id = item.Id,
                AssetId1 = item.AssetId1,
                AssetId2 = item.AssetId2,
                AssetId3 = item.AssetId3,
                AssetId4 = item.AssetId4,
                OtherAssetId = item.OtherAssetId,
                SerialNo = item.SerialNo,
                SelectedItemTypeId = item.ItemTypeId,
                SelectedItemBrandId = item.ItemBrandId,
                SelectedItemModelId = item.ItemModelId ?? 0,
                ItemDescription = item.ItemDescription,
                ItemImageUrl = item.ItemImageUrl,
                TotalAmount = item.TotalAmount,
                AvailableAmount = item.AvailableAmount,
                BorrowedAmount = item.BorrowedAmount,
                DamagedAmount = item.DamagedAmount,
                DisposedAmount = item.DisposedAmount,
                MinimumAmount = item.MinimumAmount,
                SelectedItemStatus = (ItemStatusEnum)item.ItemStatus,
                Remarks = item.Remarks,
                CreatedDate = item.CreatedDate,
                ModifiedDate = item.ModifiedDate,
                CreatedBy = item.CreatedBy,
                ModifiedBy = item.ModifiedBy,
                IsDeleted = item.IsDeleted
            };
            PopulateEditDropdowns(vm);
            return View(vm);
        }

        // ── POST: Items/Edit/5 ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateEditDropdowns(vm);
                return View(vm);
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            string itUser = User.GetUsernameLocalPart();
            item.AssetId1 = vm.AssetId1;
            item.AssetId2 = vm.AssetId2;
            item.AssetId3 = vm.AssetId3;
            item.AssetId4 = vm.AssetId4;
            item.OtherAssetId = vm.OtherAssetId;
            item.AssetId = BuildAssetIdFromParts(vm.AssetId1, vm.AssetId2, vm.AssetId3, vm.AssetId4, vm.OtherAssetId);
            item.SerialNo = vm.SerialNo;
            item.ItemTypeId = vm.SelectedItemTypeId;
            item.ItemBrandId = vm.SelectedItemBrandId;
            item.ItemModelId = vm.SelectedItemModelId == 0 ? null : vm.SelectedItemModelId;
            item.ItemDescription = vm.ItemDescription;
            item.ItemImageUrl = vm.ItemImageUrl;
            item.TotalAmount = vm.TotalAmount;
            item.MinimumAmount = vm.MinimumAmount;
            item.ItemStatus = (int)vm.SelectedItemStatus;
            item.Remarks = vm.Remarks;
            item.IsDeleted = vm.IsDeleted;
            item.ModifiedBy = itUser;
            item.ModifiedDate = DateTime.Now;

            try
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(item.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET: Items/Delete/5 ───────────────────────────────────────────────────
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemBrand).Include(i => i.ItemModel).Include(i => i.ItemType)
                .Include(i => i.BorrowHistories)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();

            var activeBorrows = item.BorrowHistories.Where(b => !b.IsDeleted).ToList();

            var vm = new ItemDeleteViewModel
            {
                Id = item.Id,
                AssetId = item.AssetId ?? string.Empty,
                ItemTypeName = item.ItemType?.Name ?? string.Empty,
                ItemBrandName = item.ItemBrand?.Name ?? string.Empty,
                ItemModelName = item.ItemModel?.Name ?? string.Empty,
                AvailableAmount = item.AvailableAmount,
                ItemStatus = (ItemStatusEnum)item.ItemStatus,
                BorrowHistoryCount = activeBorrows.Count,
                HasActiveBorrow = activeBorrows.Any(b => !b.ReturnDate.HasValue)
            };

            return View(vm);
        }

        // ── POST: Items/Delete/5 ──────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items
                .Include(i => i.BorrowHistories)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();

            // ตรวจสอบ dependency อีกครั้งฝั่ง server
            if (item.BorrowHistories.Any(b => !b.IsDeleted))
                return RedirectToAction(nameof(Delete), new { id });

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ── ItemStates ────────────────────────────────────────────────────────────
        public IActionResult ItemStates() => View();

        // ── GET: Items/Damaged/5 ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Damaged(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            if (item == null) return NotFound();

            if (item.AvailableAmount <= 0)
                return RedirectToAction(nameof(Details), new { id });

            var vm = new ItemDamagedViewModel
            {
                ItemId          = item.Id,
                ItemHeader      = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId     = item.AssetId ?? string.Empty,
                ItemStatus      = (ItemStatusEnum)item.ItemStatus,
                AvailableAmount = item.AvailableAmount,
                Itstaff         = await User.GetDisplayNameAsync(_userManager)
            };
            return View(vm);
        }

        // ── POST: Items/Damaged ───────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Damaged(ItemDamagedViewModel vm)
        {
            var item = await _context.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == vm.ItemId && !i.IsDeleted);
            if (item == null) return NotFound();

            vm.Itstaff = await User.GetDisplayNameAsync(_userManager);

            if (vm.Amount > item.AvailableAmount)
                ModelState.AddModelError(nameof(vm.Amount), "จำนวนเกินจำนวนพร้อมใช้");

            if (!ModelState.IsValid)
            {
                vm.ItemHeader      = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim();
                vm.ItemAssetId     = item.AssetId ?? string.Empty;
                vm.ItemStatus      = (ItemStatusEnum)item.ItemStatus;
                vm.AvailableAmount = item.AvailableAmount;
                return View(vm);
            }

            string username = User.GetUsernameLocalPart();
            item.ItemStatus = (int)ItemStatusEnum.Damaged;

            await StockHelper.ApplyStockChangeAsync(
                _context,
                vm.ItemId,
                (int)StockLogTypeEnum.Damage,
                deltaAvailable: -vm.Amount,
                deltaBorrowed:   0,
                deltaDamaged:   +vm.Amount,
                deltaDisposed:   0,
                createdBy:       username,
                remarks:         string.IsNullOrWhiteSpace(vm.Remarks) ? $"แจ้งเสียหาย โดย {vm.Itstaff}" : $"{vm.Remarks} (โดย {vm.Itstaff})");

            return RedirectToAction(nameof(Details), new { id = vm.ItemId });
        }

        // ── GET: Items/Repair/5 ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Repair(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            if (item == null) return NotFound();

            if (item.DamagedAmount <= 0)
                return RedirectToAction(nameof(Details), new { id });

            var vm = new ItemRepairViewModel
            {
                ItemId        = item.Id,
                ItemHeader    = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId   = item.AssetId ?? string.Empty,
                ItemStatus    = (ItemStatusEnum)item.ItemStatus,
                DamagedAmount = item.DamagedAmount,
                Itstaff       = await User.GetDisplayNameAsync(_userManager)
            };
            return View(vm);
        }

        // ── POST: Items/Repair ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repair(ItemRepairViewModel vm)
        {
            var item = await _context.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == vm.ItemId && !i.IsDeleted);
            if (item == null) return NotFound();

            vm.Itstaff = await User.GetDisplayNameAsync(_userManager);

            if (vm.Amount > item.DamagedAmount)
                ModelState.AddModelError(nameof(vm.Amount), "จำนวนเกินจำนวนที่ชำรุดอยู่");

            if (!ModelState.IsValid)
            {
                vm.ItemHeader    = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim();
                vm.ItemAssetId   = item.AssetId ?? string.Empty;
                vm.ItemStatus    = (ItemStatusEnum)item.ItemStatus;
                vm.DamagedAmount = item.DamagedAmount;
                return View(vm);
            }

            string username = User.GetUsernameLocalPart();
            item.ItemStatus = (int)ItemStatusEnum.Available;

            await StockHelper.ApplyStockChangeAsync(
                _context,
                vm.ItemId,
                (int)StockLogTypeEnum.Repair,
                deltaAvailable: +vm.Amount,
                deltaBorrowed:   0,
                deltaDamaged:   -vm.Amount,
                deltaDisposed:   0,
                createdBy:       username,
                remarks:         string.IsNullOrWhiteSpace(vm.Remarks) ? $"ซ่อมแล้ว โดย {vm.Itstaff}" : $"{vm.Remarks} (โดย {vm.Itstaff})");

            return RedirectToAction(nameof(Details), new { id = vm.ItemId });
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private static string BuildAssetId(ItemCreateViewModel vm)
            => BuildAssetIdFromParts(vm.AssetId1, vm.AssetId2, vm.AssetId3, vm.AssetId4, vm.OtherAssetId);

        private static string BuildAssetIdFromParts(string? p1, string? p2, string? p3, string? p4, string? other)
        {
            if (!string.IsNullOrWhiteSpace(p1) && !string.IsNullOrWhiteSpace(p2) &&
                !string.IsNullOrWhiteSpace(p3) && !string.IsNullOrWhiteSpace(p4))
                return $"{p1}-{p2}-{p3}-{p4}";
            return !string.IsNullOrWhiteSpace(other) ? other : string.Empty;
        }

        private async Task<string?> SaveImageAsync(ItemCreateViewModel vm)
        {
            if (vm.ItemImageFile == null || vm.ItemImageFile.Length == 0) return null;

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(vm.ItemImageFile.FileName).ToLower();
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("ItemImageFile", "อนุญาตเฉพาะไฟล์รูปภาพ (.jpg .png .gif .webp)");
                return null;
            }
            if (vm.ItemImageFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("ItemImageFile", "ขนาดไฟล์ต้องไม่เกิน 2MB");
                return null;
            }

            int count = await _context.Items.CountAsync();
            string fileName = $"IT{DateTime.Now:yyyyMMddHHmm}_{(count + 1).ToString().PadLeft(5, '0')}.jpg";
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items");
            Directory.CreateDirectory(uploadDir);

            using var stream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create);
            await vm.ItemImageFile.CopyToAsync(stream);

            return $"/uploads/items/{fileName}";
        }

        private void PopulateItemTypes(ItemCreateViewModel vm)
        {
            vm.ItemTypes = _context.ItemTypes.Where(i => !i.IsDeleted).OrderBy(i => i.Name)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name, Selected = i.Id == vm.SelectedItemTypeId }).ToList();
            vm.ItemTypes.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกประเภทอุปกรณ์ --" });
        }

        private void PopulateItemBrands(ItemCreateViewModel vm)
        {
            vm.ItemBrands = _context.ItemBrands.Where(i => !i.IsDeleted).OrderBy(i => i.Name)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name, Selected = i.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });
        }

        private void PopulateItemModels(ItemCreateViewModel vm)
        {
            vm.ItemModels = _context.ItemModels.Where(i => !i.IsDeleted).OrderBy(i => i.Name)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name, Selected = i.Id == vm.SelectedItemModelId }).ToList();
            vm.ItemModels.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกรุ่น --" });
        }

        private void PopulateItemStatuses(ItemCreateViewModel vm)
        {
            vm.ItemStatuses = Enum.GetValues(typeof(ItemStatusEnum)).Cast<ItemStatusEnum>()
                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.GetDisplayName(), Selected = e == vm.SelectedItemStatus }).ToList();
            vm.ItemStatuses.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกสถานะ --" });
        }

        private void PopulateCascadeMaps(ItemCreateViewModel vm)
        {
            var links = _context.ItemTypeToBrands.ToList();
            vm.TypeToBrandsMap = links.GroupBy(l => l.ItemTypeId).ToDictionary(g => g.Key, g => g.Select(l => l.ItemBrandId).ToList());
            vm.BrandToTypesMap = links.GroupBy(l => l.ItemBrandId).ToDictionary(g => g.Key, g => g.Select(l => l.ItemTypeId).ToList());
            vm.BrandToModelsMap = _context.ItemModels.Where(m => !m.IsDeleted).OrderBy(m => m.Name)
                .GroupBy(m => m.ItemBrandId)
                .ToDictionary(g => g.Key, g => g.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList());
        }

        private void PopulateEditDropdowns(ItemEditViewModel vm)
        {
            vm.ItemTypes = _context.ItemTypes.Where(i => !i.IsDeleted).OrderBy(i => i.Name)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name, Selected = i.Id == vm.SelectedItemTypeId }).ToList();
            vm.ItemTypes.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกประเภทอุปกรณ์ --" });

            vm.ItemBrands = _context.ItemBrands.Where(i => !i.IsDeleted).OrderBy(i => i.Name)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name, Selected = i.Id == vm.SelectedItemBrandId }).ToList();
            vm.ItemBrands.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกยี่ห้อ --" });

            vm.ItemModels = _context.ItemModels.Where(i => !i.IsDeleted).OrderBy(i => i.Name)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name, Selected = i.Id == vm.SelectedItemModelId }).ToList();
            vm.ItemModels.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกรุ่น --" });

            vm.ItemStatuses = Enum.GetValues(typeof(ItemStatusEnum)).Cast<ItemStatusEnum>()
                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.GetDisplayName(), Selected = e == vm.SelectedItemStatus }).ToList();
        }

        private bool ItemExists(int id) => _context.Items.Any(e => e.Id == id);
    }
}
