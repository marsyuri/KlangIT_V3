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
    public class BorrowHistoriesController : Controller
    {
        private readonly IBorrowService _borrowService;
        private readonly IItemService _itemService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BorrowHistoriesController(
            IBorrowService borrowService,
            IItemService itemService,
            UserManager<ApplicationUser> userManager)
        {
            _borrowService = borrowService;
            _itemService = itemService;
            _userManager = userManager;
        }

        // ── GET: BorrowHistories ──────────────────────────────────────────────────
        public async Task<IActionResult> Index()
            => View(await _borrowService.GetIndexAsync());

        // ── GET: BorrowHistories/Details/5 ───────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _borrowService.GetDetailsAsync(id.Value);
            if (bh == null) return NotFound();

            return View(bh);
        }

        // ── GET: BorrowHistories/Create ───────────────────────────────────────────
        public async Task<IActionResult> Create(int? itemId)
        {
            if (itemId == null) return NotFound();

            var item = await _itemService.GetItemWithRelationsAsync(itemId.Value);
            if (item == null) return NotFound();

            var vm = new BorrowCreateViewModel
            {
                ItemId             = item.Id,
                ItemHeader         = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId        = item.AssetId ?? string.Empty,
                ItemStatus         = (ItemStatusEnum)item.ItemStatus,
                BorrowDate         = DateTime.Now,
                ExpectedReturnDate = DateTime.Now,
                Itstaff            = await User.GetDisplayNameAsync(_userManager)
            };
            await PopulateBorrowCreateDropdownsAsync(vm);
            return View(vm);
        }

        // ── POST: BorrowHistories/Create ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowCreateViewModel bhVM)
        {
            bhVM.Itstaff = await User.GetDisplayNameAsync(_userManager);

            if (!ModelState.IsValid)
            {
                await PopulateBorrowCreateDropdownsAsync(bhVM);
                return View(bhVM);
            }

            if (bhVM.BorrowDate.Date == DateTime.Now.Date)
                bhVM.BorrowDate = DateTime.Now;

            string username = User.GetUsernameLocalPart();
            await _borrowService.CreateBorrowAsync(
                itemId:        bhVM.ItemId,
                borrowerUser:  bhVM.RequestUser,
                departmentId:  bhVM.SelectedDepartmentId,
                sectionId:     bhVM.SelectedSectionId == 0 ? null : bhVM.SelectedSectionId,
                isPermanent:   bhVM.IsPermanentBorrow,
                borrowDate:    bhVM.BorrowDate,
                dueDate:       bhVM.HasExpectedReturnDate ? bhVM.ExpectedReturnDate : null,
                amount:        bhVM.Amount,
                displayName:   bhVM.Itstaff,
                username:      username);

            return RedirectToAction(nameof(ItemsController.Details), "Items", new { id = bhVM.ItemId });
        }

        // ── GET: BorrowHistories/Return ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _borrowService.GetActiveBorrowAsync(id.Value);
            if (bh == null) return NotFound();

            var item = await _itemService.GetItemWithRelationsAsync(bh.ItemId);
            if (item == null || item.IsDeleted) return NotFound();

            var vm = new ReturnCreateViewModel
            {
                Id                   = bh.Id,
                ItemId               = item.Id,
                ItemHeader           = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId          = item.AssetId ?? string.Empty,
                ItemStatus           = (ItemStatusEnum)item.ItemStatus,
                RequestUser          = bh.BorrowerUser,
                SelectedDepartmentId = bh.BorrowerDepartmentId,
                SelectedSectionId    = bh.BorrowerSectionId ?? 0,
                BorrowDate           = bh.BorrowDate,
                Itstaff              = await User.GetDisplayNameAsync(_userManager)
            };
            await PopulateReturnDropdownsAsync(vm);
            return View(vm);
        }

        // ── POST: BorrowHistories/Return ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnCreateViewModel bhVM)
        {
            bhVM.Itstaff = await User.GetDisplayNameAsync(_userManager);

            if (!ModelState.IsValid)
            {
                await PopulateReturnDropdownsAsync(bhVM);
                return View(bhVM);
            }

            string username = User.GetUsernameLocalPart();
            bool ok = await _borrowService.ProcessReturnAsync(bhVM.Id, bhVM.ReturnAmount, bhVM.Itstaff, username);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(ItemsController.Details), "Items", new { id = bhVM.ItemId });
        }

        // ── GET: BorrowHistories/Edit/5 ───────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _borrowService.GetForEditAsync(id.Value);
            if (bh == null) return NotFound();

            var vm = new BorrowHistoryEditViewModel
            {
                Id                    = bh.Id,
                RequestUser           = bh.BorrowerUser,
                SelectedDepartmentId  = bh.BorrowerDepartmentId,
                SelectedSectionId     = bh.BorrowerSectionId ?? 0,
                IsPermanentBorrow     = bh.IsPermanentBorrow,
                BorrowDate            = bh.BorrowDate,
                HasExpectedReturnDate = bh.DueDate.HasValue,
                ExpectedReturnDate    = bh.DueDate,
                IsReturn              = bh.ReturnDate.HasValue,
                ReturnDate            = bh.ReturnDate,
                Itstaff               = bh.ReturnDate.HasValue ? bh.ReturnItname : bh.BorrowItname,
                Amount                = bh.Amount
            };
            await PopulateBorrowEditDropdownsAsync(vm);
            return View(vm);
        }

        // ── POST: BorrowHistories/Edit/5 ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BorrowHistoryEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateBorrowEditDropdownsAsync(vm);
                return View(vm);
            }

            var bh = await _borrowService.GetForEditAsync(id);
            if (bh == null) return NotFound();

            bh.BorrowerUser         = vm.RequestUser;
            bh.BorrowerDepartmentId = vm.SelectedDepartmentId;
            bh.BorrowerSectionId    = vm.SelectedSectionId == 0 ? null : vm.SelectedSectionId;
            bh.IsPermanentBorrow    = vm.IsPermanentBorrow;
            bh.BorrowDate           = vm.BorrowDate;
            bh.DueDate              = vm.HasExpectedReturnDate ? vm.ExpectedReturnDate : null;
            bh.ReturnDate           = vm.IsReturn ? (vm.ReturnDate ?? DateTime.Now) : null;
            if (vm.IsReturn) bh.ReturnItname = vm.Itstaff;
            else             bh.BorrowItname = vm.Itstaff;
            bh.Amount               = vm.Amount;

            string username = User.GetUsernameLocalPart();
            try
            {
                await _borrowService.UpdateBorrowAsync(bh, username);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET: BorrowHistories/Delete/5 ─────────────────────────────────────────
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _borrowService.GetForDeleteAsync(id.Value);
            if (bh == null) return NotFound();

            var vm = new BorrowHistoryDeleteViewModel
            {
                Id             = bh.Id,
                ItemId         = bh.ItemId,
                ItemHeader     = $"{bh.Item?.ItemType?.Name} {bh.Item?.ItemBrand?.Name} {bh.Item?.ItemModel?.Name}".Trim(),
                ItemAssetId    = bh.Item?.AssetId  ?? string.Empty,
                RequestUser    = bh.BorrowerUser,
                DepartmentName = bh.BorrowerDepartment?.Name ?? string.Empty,
                SectionName    = bh.BorrowerSection?.Name    ?? string.Empty,
                BorrowDate     = bh.BorrowDate,
                IsReturn       = bh.ReturnDate.HasValue
            };

            return View(vm);
        }

        // ── POST: BorrowHistories/Delete/5 ────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _borrowService.DeleteBorrowAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ── Private helpers (UI concerns only) ────────────────────────────────────

        private async Task PopulateBorrowCreateDropdownsAsync(BorrowCreateViewModel vm)
        {
            vm.Departments = BuildDepartmentList(await _borrowService.GetActiveDepartmentsAsync(), vm.SelectedDepartmentId);
            vm.Sections    = BuildSectionList(await _borrowService.GetActiveSectionsAsync(), vm.SelectedSectionId);
        }

        private async Task PopulateReturnDropdownsAsync(ReturnCreateViewModel vm)
        {
            vm.Departments = BuildDepartmentList(await _borrowService.GetActiveDepartmentsAsync(), vm.SelectedDepartmentId);
            vm.Sections    = BuildSectionList(await _borrowService.GetActiveSectionsAsync(), vm.SelectedSectionId);
        }

        private async Task PopulateBorrowEditDropdownsAsync(BorrowHistoryEditViewModel vm)
        {
            vm.Departments = BuildDepartmentList(await _borrowService.GetActiveDepartmentsAsync(), vm.SelectedDepartmentId);
            vm.Sections    = BuildSectionList(await _borrowService.GetActiveSectionsAsync(), vm.SelectedSectionId);
        }

        private static List<SelectListItem> BuildDepartmentList(IEnumerable<Department> source, int selectedId)
        {
            var list = source
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == selectedId })
                .ToList();
            list.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกฝ่าย/กลุ่มงาน --" });
            return list;
        }

        private static List<SelectListItem> BuildSectionList(IEnumerable<Section> source, int selectedId)
        {
            var list = source
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name, Selected = s.Id == selectedId })
                .ToList();
            list.Insert(0, new SelectListItem { Value = "0", Text = "-- เลือกหน่วยงาน --", Selected = selectedId == 0 });
            return list;
        }
    }
}
