using KlangIT_V3.Data;
using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using KlangIT_V3.Services;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KlangIT_V3.Controllers
{
    public class ItemsController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public ItemsController(
            IItemService itemService,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager)
        {
            _itemService = itemService;
            _env = env;
            _userManager = userManager;
        }

        // ── GET: Items ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string sortOrder, string filterTypeId, string filterBrandId, string filterModelId, string searchBox)
        {
            sortOrder ??= "asset_asc";
            ViewBag.CurrentSort     = sortOrder;
            ViewBag.SortByAssetId   = sortOrder == "asset_asc"   ? "asset_desc"   : "asset_asc";
            ViewBag.SortByItemType  = sortOrder == "type_asc"    ? "type_desc"    : "type_asc";
            ViewBag.SortByItemBrand = sortOrder == "brand_asc"   ? "brand_desc"   : "brand_asc";
            ViewBag.SortByItemModel = sortOrder == "model_asc"   ? "model_desc"   : "model_asc";
            ViewBag.SortByAvlNo     = sortOrder == "avl_asc"     ? "avl_desc"     : "avl_asc";
            ViewBag.SortByModDate   = sortOrder == "moddate_asc" ? "moddate_desc" : "moddate_asc";
            ViewBag.CurrentSearch   = searchBox;

            int? typeId  = int.TryParse(filterTypeId,  out var t) ? t : null;
            int? brandId = int.TryParse(filterBrandId, out var b) ? b : null;
            int? modelId = int.TryParse(filterModelId, out var m) ? m : null;

            var maps = await _itemService.GetCascadeMapsAsync();
            ViewBag.TypeToBrandsMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(maps.TypeToBrands);
            ViewBag.BrandToTypesMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(maps.BrandToTypes);
            ViewBag.BrandToModelsMapJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                maps.BrandToModels.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Select(mdl => new { Value = mdl.Id.ToString(), Text = mdl.Name }).ToList()));

            ViewBag.ItemTypes = (await _itemService.GetActiveItemTypesAsync())
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id.ToString() == filterTypeId })
                .ToList();
            ViewBag.ItemBrands = (await _itemService.GetActiveItemBrandsAsync())
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id.ToString() == filterBrandId })
                .ToList();
            ViewBag.ItemModels = (await _itemService.GetActiveItemModelsAsync())
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id.ToString() == filterModelId })
                .ToList();

            var items = await _itemService.GetFilteredItemsAsync(sortOrder, typeId, brandId, modelId, searchBox);
            return View(items);
        }

        // ── GET: Items/Details/5 ──────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _itemService.GetItemDetailsAsync(id.Value);
            if (item == null) return NotFound();

            var stockLogs = await _itemService.GetStockLogsAsync(item.Id);

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
        public async Task<IActionResult> Create()
        {
            var vm = new ItemCreateViewModel { TotalAmount = 1, MinimumAmount = 0 };
            await PopulateCreateDropdownsAsync(vm);
            return View(vm);
        }

        // ── POST: Items/Create ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCreateViewModel itemVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateDropdownsAsync(itemVM);
                return View(itemVM);
            }

            string? imageUrl = await SaveImageAsync(itemVM.ItemImageFile);
            if (!ModelState.IsValid)
            {
                await PopulateCreateDropdownsAsync(itemVM);
                return View(itemVM);
            }

            string username = User.GetUsernameLocalPart();
            int initialAmount = itemVM.IsBulk ? itemVM.TotalAmount : 1;
            var selectedStatus = itemVM.SelectedItemStatus ?? ItemStatusEnum.Available;

            var item = new Item
            {
                IsBulk          = itemVM.IsBulk,
                AssetId         = BuildAssetId(itemVM),
                AssetId1        = itemVM.AssetId1,
                AssetId2        = itemVM.AssetId2,
                AssetId3        = itemVM.AssetId3,
                AssetId4        = itemVM.AssetId4,
                OtherAssetId    = itemVM.OtherAssetId,
                SerialNo        = itemVM.SerialNo,
                ItemTypeId      = itemVM.SelectedItemTypeId ?? 0,
                ItemBrandId     = itemVM.SelectedItemBrandId ?? 0,
                ItemModelId     = itemVM.SelectedItemModelId,
                ItemDescription = itemVM.ItemDescription,
                ItemImageUrl    = imageUrl,
                MinimumAmount   = itemVM.MinimumAmount,
                Remarks         = itemVM.Remarks
            };

            await _itemService.CreateItemAsync(item, initialAmount, selectedStatus, username);

            return RedirectToAction(nameof(Index), new { sortOrder = "moddate_desc" });
        }

        // ── GET: Items/Edit/5 ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _itemService.GetItemWithRelationsAsync(id.Value);
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
            await PopulateEditDropdownsAsync(vm);
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
                await PopulateEditDropdownsAsync(vm);
                return View(vm);
            }

            var item = await _itemService.GetItemAsync(id);
            if (item == null) return NotFound();

            string? newImageUrl = await SaveImageAsync(vm.ItemImageFile);
            if (!ModelState.IsValid)
            {
                await PopulateEditDropdownsAsync(vm);
                return View(vm);
            }

            string username = User.GetUsernameLocalPart();
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
            item.ItemImageUrl = newImageUrl ?? vm.ItemImageUrl;
            item.TotalAmount = vm.TotalAmount;
            item.MinimumAmount = vm.MinimumAmount;
            item.ItemStatus = (int)vm.SelectedItemStatus;
            item.Remarks = vm.Remarks;
            item.IsDeleted = vm.IsDeleted;

            try
            {
                await _itemService.UpdateItemAsync(item, username);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index), new { sortOrder = "moddate_desc" });
        }

        // ── GET: Items/Delete/5 ───────────────────────────────────────────────────
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _itemService.GetForDeleteAsync(id.Value);
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
            var outcome = await _itemService.DeleteItemAsync(id);
            return outcome switch
            {
                DeleteOutcome.NotFound        => NotFound(),
                DeleteOutcome.HasDependencies => RedirectToAction(nameof(Delete), new { id }),
                _                             => RedirectToAction(nameof(Index))
            };
        }

        // ── ItemStates ────────────────────────────────────────────────────────────
        public IActionResult ItemStates() => View();

        // ── GET: Items/Damaged/5 ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Damaged(int? id)
        {
            if (id == null) return NotFound();

            var item = await _itemService.GetItemWithRelationsAsync(id.Value);
            if (item == null || item.IsDeleted) return NotFound();

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
            var item = await _itemService.GetItemWithRelationsAsync(vm.ItemId);
            if (item == null || item.IsDeleted) return NotFound();

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
            await _itemService.MarkDamagedAsync(vm.ItemId, vm.Amount, vm.Remarks, username, vm.Itstaff);

            return RedirectToAction(nameof(Details), new { id = vm.ItemId });
        }

        // ── GET: Items/Repair/5 ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Repair(int? id)
        {
            if (id == null) return NotFound();

            var item = await _itemService.GetItemWithRelationsAsync(id.Value);
            if (item == null || item.IsDeleted) return NotFound();

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
            var item = await _itemService.GetItemWithRelationsAsync(vm.ItemId);
            if (item == null || item.IsDeleted) return NotFound();

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
            await _itemService.MarkRepairedAsync(vm.ItemId, vm.Amount, vm.Remarks, username, vm.Itstaff);

            return RedirectToAction(nameof(Details), new { id = vm.ItemId });
        }

        // ── Private helpers (UI concerns only) ────────────────────────────────────

        private static string BuildAssetId(ItemCreateViewModel vm)
            => BuildAssetIdFromParts(vm.AssetId1, vm.AssetId2, vm.AssetId3, vm.AssetId4, vm.OtherAssetId);

        private static string BuildAssetIdFromParts(string? p1, string? p2, string? p3, string? p4, string? other)
        {
            if (!string.IsNullOrWhiteSpace(p1) && !string.IsNullOrWhiteSpace(p2) &&
                !string.IsNullOrWhiteSpace(p3) && !string.IsNullOrWhiteSpace(p4))
                return $"{p1}-{p2}-{p3}-{p4}";
            return !string.IsNullOrWhiteSpace(other) ? other : string.Empty;
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("ItemImageFile", "อนุญาตเฉพาะไฟล์รูปภาพ (.jpg .png .gif .webp)");
                return null;
            }
            if (file.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("ItemImageFile", "ขนาดไฟล์ต้องไม่เกิน 2MB");
                return null;
            }

            int count = await _itemService.GetItemCountAsync();
            string fileName = $"IT{DateTime.Now:yyyyMMddHHmm}_{(count + 1).ToString().PadLeft(5, '0')}.jpg";
            string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "items");
            Directory.CreateDirectory(uploadDir);

            using var stream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/items/{fileName}";
        }

        private async Task PopulateCreateDropdownsAsync(ItemCreateViewModel vm)
        {
            var types  = await _itemService.GetActiveItemTypesAsync();
            var brands = await _itemService.GetActiveItemBrandsAsync();
            var models = await _itemService.GetActiveItemModelsAsync();
            var maps   = await _itemService.GetCascadeMapsAsync();

            vm.ItemTypes = BuildSelectList(types.Select(t => (t.Id, t.Name)), vm.SelectedItemTypeId, "-- เลือกประเภทอุปกรณ์ --");
            vm.ItemBrands = BuildSelectList(brands.Select(b => (b.Id, b.Name)), vm.SelectedItemBrandId, "-- เลือกยี่ห้อ --");
            vm.ItemModels = BuildSelectList(models.Select(m => (m.Id, m.Name)), vm.SelectedItemModelId, "-- เลือกรุ่น --");
            vm.ItemStatuses = BuildItemStatusList(vm.SelectedItemStatus, withBlank: true);
            vm.TypeToBrandsMap = maps.TypeToBrands;
            vm.BrandToTypesMap = maps.BrandToTypes;
            vm.BrandToModelsMap = maps.BrandToModels.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList());
        }

        private async Task PopulateEditDropdownsAsync(ItemEditViewModel vm)
        {
            var types  = await _itemService.GetActiveItemTypesAsync();
            var brands = await _itemService.GetActiveItemBrandsAsync();
            var models = await _itemService.GetActiveItemModelsAsync();
            var maps   = await _itemService.GetCascadeMapsAsync();

            vm.ItemTypes = BuildSelectList(types.Select(t => (t.Id, t.Name)), vm.SelectedItemTypeId, "-- เลือกประเภทอุปกรณ์ --");
            vm.ItemBrands = BuildSelectList(brands.Select(b => (b.Id, b.Name)), vm.SelectedItemBrandId, "-- เลือกยี่ห้อ --");
            vm.ItemModels = BuildSelectList(models.Select(m => (m.Id, m.Name)), vm.SelectedItemModelId, "-- เลือกรุ่น --");
            vm.ItemStatuses = BuildItemStatusList(vm.SelectedItemStatus, withBlank: false);
            vm.TypeToBrandsMap = maps.TypeToBrands;
            vm.BrandToTypesMap = maps.BrandToTypes;
            vm.BrandToModelsMap = maps.BrandToModels.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList());
        }

        private static List<SelectListItem> BuildSelectList(IEnumerable<(int Id, string Name)> source, int? selectedId, string placeholder)
        {
            var list = source
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == selectedId })
                .ToList();
            list.Insert(0, new SelectListItem { Value = "", Text = placeholder });
            return list;
        }

        private static List<SelectListItem> BuildItemStatusList(ItemStatusEnum? selected, bool withBlank)
        {
            var list = Enum.GetValues(typeof(ItemStatusEnum)).Cast<ItemStatusEnum>()
                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.GetDisplayName(), Selected = e == selected })
                .ToList();
            if (withBlank) list.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกสถานะ --" });
            return list;
        }
    }
}
