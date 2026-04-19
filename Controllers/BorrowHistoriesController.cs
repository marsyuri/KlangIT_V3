using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;
using KlangIT_V3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Controllers
{
    public class BorrowHistoriesController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public BorrowHistoriesController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // ── GET: BorrowHistories ──────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var list = await _context.BorrowHistories
                .Include(b => b.Item).ThenInclude(i => i!.ItemType)
                .Include(b => b.Item).ThenInclude(i => i!.ItemBrand)
                .Include(b => b.RequestDepartment)
                .Include(b => b.RequestSection)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();
            return View(list);
        }

        // ── GET: BorrowHistories/Details/5 ───────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _context.BorrowHistories
                .Include(b => b.Item).ThenInclude(i => i!.ItemType)
                .Include(b => b.Item).ThenInclude(i => i!.ItemBrand)
                .Include(b => b.Item).ThenInclude(i => i!.ItemModel)
                .Include(b => b.RequestDepartment)
                .Include(b => b.RequestSection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bh == null) return NotFound();

            return View(bh);
        }

        // ── GET: BorrowHistories/Create ───────────────────────────────────────────
        public async Task<IActionResult> Create(int? itemId)
        {
            if (itemId == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return NotFound();

            var vm = new BorrowCreateViewModel
            {
                ItemId         = item.Id,
                ItemHeader     = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId    = item.AssetId ?? string.Empty,
                ItemStatus     = item.ItemStatus,
                BorrowDate     = DateTime.Now,
                ExpectedReturnDate = DateTime.Now
            };
            PopulateDepartments(vm);
            PopulateSections(vm);
            return View(vm);
        }

        // ── POST: BorrowHistories/Create ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowCreateViewModel bhVM)
        {
            if (!ModelState.IsValid)
            {
                PopulateDepartments(bhVM);
                PopulateSections(bhVM);
                return View(bhVM);
            }

            if (bhVM.BorrowDate.Date == DateTime.Now.Date)
                bhVM.BorrowDate = DateTime.Now;

            string itUser = Utility.GetCurrentUserName();
            var bh = new BorrowHistory
            {
                ItemId               = bhVM.ItemId,
                RequestUser          = bhVM.RequestUser,
                RequestSectionId     = bhVM.SelectedSectionId == 0 ? null : bhVM.SelectedSectionId,
                RequestDepartmentId  = bhVM.SelectedDepartmentId,
                IsPermanentBorrow    = bhVM.IsPermanentBorrow,
                BorrowDate           = bhVM.BorrowDate,
                HasExpectedReturnDate= bhVM.HasExpectedReturnDate,
                ExpectedReturnDate   = bhVM.ExpectedReturnDate,
                Itstaff              = bhVM.Itstaff,
                Amount               = bhVM.Amount,
                CreatedDate          = DateTime.Now,
                ModifiedDate         = DateTime.Now,
                CreatedBy            = itUser,
                ModifiedBy           = itUser,
                IsDeleted            = false
            };
            _context.BorrowHistories.Add(bh);

            var item = await _context.Items.FindAsync(bhVM.ItemId);
            if (item == null) return NotFound();
            item.ItemStatus      = ItemStatusEnum.Borrowed;
            item.AvailableAmount -= bhVM.Amount;
            item.BorrowedAmount  += bhVM.Amount;
            item.ModifiedBy      = itUser;
            item.ModifiedDate    = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ItemsController.Details), "Items", new { id = bhVM.ItemId });
        }

        // ── GET: BorrowHistories/Return ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _context.BorrowHistories
                .Where(b => b.Id == id && !b.IsDeleted && !b.IsReturn)
                .SingleOrDefaultAsync();
            if (bh == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemType).Include(i => i.ItemBrand).Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == bh.ItemId && !i.IsDeleted);
            if (item == null) return NotFound();

            var vm = new ReturnCreateViewModel
            {
                Id                  = bh.Id,
                ItemId              = item.Id,
                ItemHeader          = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId         = item.AssetId ?? string.Empty,
                ItemStatus          = item.ItemStatus,
                RequestUser         = bh.RequestUser,
                SelectedDepartmentId= bh.RequestDepartmentId,
                SelectedSectionId   = bh.RequestSectionId ?? 0,
                BorrowDate          = bh.BorrowDate
            };
            PopulateDepartments(vm);
            PopulateSections(vm);
            return View(vm);
        }

        // ── POST: BorrowHistories/Return ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnCreateViewModel bhVM)
        {
            if (!ModelState.IsValid)
            {
                PopulateDepartments(bhVM);
                PopulateSections(bhVM);
                return View(bhVM);
            }

            var bh = await _context.BorrowHistories
                .Where(b => b.Id == bhVM.Id && !b.IsDeleted && !b.IsReturn)
                .SingleOrDefaultAsync();
            if (bh == null) return NotFound();

            string itUser = Utility.GetCurrentUserName();
            bh.IsPermanentBorrow = false;
            bh.IsReturn          = bh.Amount == bhVM.ReturnAmount;
            bh.ReturnDate        = DateTime.Now;
            bh.DurationDays      = (bh.ReturnDate.Value - bh.BorrowDate).Days;
            bh.Itstaff           = bhVM.Itstaff;
            bh.Amount           -= bhVM.ReturnAmount;
            bh.ModifiedBy        = itUser;
            bh.ModifiedDate      = DateTime.Now;

            var item = await _context.Items.FindAsync(bhVM.ItemId);
            if (item == null) return NotFound();
            item.ItemStatus      = ItemStatusEnum.Available;
            item.AvailableAmount += bhVM.ReturnAmount;
            item.BorrowedAmount  -= bhVM.ReturnAmount;
            item.ModifiedBy      = itUser;
            item.ModifiedDate    = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ItemsController.Details), "Items", new { id = bhVM.ItemId });
        }

        // ── GET: BorrowHistories/Edit/5 ───────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _context.BorrowHistories.FindAsync(id);
            if (bh == null) return NotFound();

            var vm = new BorrowHistoryEditViewModel
            {
                Id                   = bh.Id,
                RequestUser          = bh.RequestUser,
                SelectedDepartmentId = bh.RequestDepartmentId,
                SelectedSectionId    = bh.RequestSectionId ?? 0,
                IsPermanentBorrow    = bh.IsPermanentBorrow,
                BorrowDate           = bh.BorrowDate,
                HasExpectedReturnDate= bh.HasExpectedReturnDate,
                ExpectedReturnDate   = bh.ExpectedReturnDate,
                IsReturn             = bh.IsReturn,
                ReturnDate           = bh.ReturnDate,
                Itstaff              = bh.Itstaff,
                Amount               = bh.Amount
            };
            PopulateDepartmentsEdit(vm);
            PopulateSectionsEdit(vm);
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
                PopulateDepartmentsEdit(vm);
                PopulateSectionsEdit(vm);
                return View(vm);
            }

            var bh = await _context.BorrowHistories.FindAsync(id);
            if (bh == null) return NotFound();

            string itUser = Utility.GetCurrentUserName();
            bh.RequestUser           = vm.RequestUser;
            bh.RequestDepartmentId   = vm.SelectedDepartmentId;
            bh.RequestSectionId      = vm.SelectedSectionId == 0 ? null : vm.SelectedSectionId;
            bh.IsPermanentBorrow     = vm.IsPermanentBorrow;
            bh.BorrowDate            = vm.BorrowDate;
            bh.HasExpectedReturnDate = vm.HasExpectedReturnDate;
            bh.ExpectedReturnDate    = vm.ExpectedReturnDate;
            bh.IsReturn              = vm.IsReturn;
            bh.ReturnDate            = vm.ReturnDate;
            bh.Itstaff               = vm.Itstaff;
            bh.Amount                = vm.Amount;
            bh.ModifiedBy            = itUser;
            bh.ModifiedDate          = DateTime.Now;

            try
            {
                _context.Update(bh);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowHistoryExists(bh.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET: BorrowHistories/Delete/5 ─────────────────────────────────────────
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bh = await _context.BorrowHistories
                .Include(b => b.Item).ThenInclude(i => i!.ItemType)
                .Include(b => b.Item).ThenInclude(i => i!.ItemBrand)
                .Include(b => b.Item).ThenInclude(i => i!.ItemModel)
                .Include(b => b.RequestDepartment)
                .Include(b => b.RequestSection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bh == null) return NotFound();

            var vm = new BorrowHistoryDeleteViewModel
            {
                Id             = bh.Id,
                ItemId         = bh.ItemId,
                ItemHeader     = $"{bh.Item?.ItemType?.Name} {bh.Item?.ItemBrand?.Name} {bh.Item?.ItemModel?.Name}".Trim(),
                ItemAssetId    = bh.Item?.AssetId  ?? string.Empty,
                RequestUser    = bh.RequestUser,
                DepartmentName = bh.RequestDepartment?.Name ?? string.Empty,
                SectionName    = bh.RequestSection?.Name    ?? string.Empty,
                BorrowDate     = bh.BorrowDate,
                IsReturn       = bh.IsReturn
            };

            return View(vm);
        }

        // ── POST: BorrowHistories/Delete/5 ────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bh = await _context.BorrowHistories.FindAsync(id);
            if (bh != null) _context.BorrowHistories.Remove(bh);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowHistoryExists(int id) => _context.BorrowHistories.Any(e => e.Id == id);

        // ── Populate helpers ──────────────────────────────────────────────────────
        private void PopulateDepartments(BorrowCreateViewModel model)
        {
            model.Departments = GetDepartmentList(model.SelectedDepartmentId);
        }
        private void PopulateDepartments(ReturnCreateViewModel model)
        {
            model.Departments = GetDepartmentList(model.SelectedDepartmentId);
        }
        private void PopulateDepartmentsEdit(BorrowHistoryEditViewModel model)
        {
            model.Departments = GetDepartmentList(model.SelectedDepartmentId);
        }

        private void PopulateSections(BorrowCreateViewModel model)
        {
            model.Sections = GetSectionList(model.SelectedSectionId);
        }
        private void PopulateSections(ReturnCreateViewModel model)
        {
            model.Sections = GetSectionList(model.SelectedSectionId);
        }
        private void PopulateSectionsEdit(BorrowHistoryEditViewModel model)
        {
            model.Sections = GetSectionList(model.SelectedSectionId);
        }

        private List<SelectListItem> GetDepartmentList(int selectedId)
        {
            var list = _context.Departments.Where(d => !d.IsDeleted).OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == selectedId }).ToList();
            list.Insert(0, new SelectListItem { Value = "", Text = "-- เลือกฝ่าย/กลุ่มงาน --" });
            return list;
        }

        private List<SelectListItem> GetSectionList(int selectedId)
        {
            var list = _context.Sections.Where(s => !s.IsDeleted).OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name, Selected = s.Id == selectedId }).ToList();
            list.Insert(0, new SelectListItem { Value = "0", Text = "-- เลือกหน่วยงาน --", Selected = selectedId == 0 });
            return list;
        }
    }
}
