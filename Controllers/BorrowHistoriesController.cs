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
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace KlangIT_V3.Controllers
{
    public class BorrowHistoriesController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public BorrowHistoriesController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // GET: BorrowHistories
        public async Task<IActionResult> Index()
        {
            var itLptWarehouseContext = _context.BorrowHistories.Include(b => b.Item).Include(b => b.RequestDepartment).Include(b => b.RequestSection);
            return View(await itLptWarehouseContext.ToListAsync());
        }

        // GET: BorrowHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowHistory = await _context.BorrowHistories
                .Include(b => b.Item)
                .Include(b => b.RequestDepartment)
                .Include(b => b.RequestSection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrowHistory == null)
            {
                return NotFound();
            }

            return View(borrowHistory);
        }

        // GET: BorrowHistories/Create
        public async Task<IActionResult> Create(int? itemId)
        {
            if (itemId == null)
            {
                return NotFound();
            }

            Item? item = await _context.Items
                .Include(i => i.ItemType)
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
            {
                return NotFound();
            }

            BorrowCreateViewModel viewModel = new BorrowCreateViewModel
            {
                ItemId = item.Id,
                ItemHeader = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId = item.AssetId == null ? string.Empty : item.AssetId,
                ItemStatus = item.ItemStatus,
                BorrowDate = DateTime.Now,
                ExpectedReturnDate = DateTime.Now
            };
            PopulateDepartments(viewModel);
            PopulateSections(viewModel);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BorrowHistory? bh = await _context.BorrowHistories
                .Where(bh => bh.Id == id && !bh.IsDeleted && !bh.IsReturn)
                .SingleOrDefaultAsync();
            if (bh == null)
            {
                return NotFound();
            }

            Item? item = await _context.Items
                .Include(i => i.ItemType)
                .Include(i => i.ItemBrand)
                .Include(i => i.ItemModel)
                .SingleOrDefaultAsync(i => i.Id == bh.ItemId && !i.IsDeleted);
            if (item == null)
            {
                return NotFound();
            }

            ReturnCreateViewModel viewModel = new ReturnCreateViewModel
            {
                ItemId = item.Id,
                ItemHeader = $"{item.ItemType?.Name} {item.ItemBrand?.Name} {item.ItemModel?.Name}".Trim(),
                ItemAssetId = item.AssetId == null ? string.Empty : item.AssetId,
                ItemStatus = item.ItemStatus,
                RequestUser = bh.RequestUser,
                SelectedDepartmentId = bh.RequestDepartmentId,
                SelectedSectionId = bh.RequestSectionId ?? 0,
                BorrowDate = bh.BorrowDate
            };
            PopulateDepartments(viewModel);
            PopulateSections(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnCreateViewModel bhVM)
        {
            if (bhVM == null)
            {
                return NotFound();
            }
            if (bhVM.Id == 0)
            {
                return NotFound();
            }
            string itUser = Utility.GetCurrentUserName();
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
                return View(bhVM);
            }
            BorrowHistory? bh = await _context.BorrowHistories
            .Where(bh => bh.Id == bhVM.Id && !bh.IsDeleted && !bh.IsReturn)
            .SingleOrDefaultAsync();
            if (bh == null)
            {
                return NotFound();
            }
            bh.IsPermanentBorrow = false;
            bh.IsReturn = (bh.Amount == bhVM.ReturnAmount); // has bug เพราะไม่ได้เก็บ borrow amount ครั้งแรก กับ borrow ล่าสุด / บังคับเวลาคืนต้องคืนให้หมด
            bh.ReturnDate = DateTime.Now;
            bh.DurationDays = (bh.ReturnDate.Value - bh.BorrowDate).Days;
            bh.Itstaff = bhVM.Itstaff;
            bh.Amount -= bhVM.ReturnAmount;
            bh.ModifiedBy = itUser;
            bh.ModifiedDate = DateTime.Now;

            Item? item = await _context.Items.FindAsync(bhVM.ItemId);
            if (item == null)
            {
                return NotFound();
            }
            item.ItemStatus = ItemStatusEnum.Available;
            item.AvailableAmount += bhVM.ReturnAmount;
            item.BorrowedAmount -= bhVM.ReturnAmount;
            item.ModifiedBy = itUser;
            item.ModifiedDate = DateTime.Now;

            StockLog? latestLog = await _context.StockLogs
                .Where(sl => sl.ItemId == bhVM.ItemId)
                .OrderByDescending(sl => sl.Id)
                .FirstOrDefaultAsync();
            if (latestLog == null)
            {
                return NotFound();
            }
            int runNo = latestLog.RunningNo + 1;
            string logNoLeftPadded = runNo.ToString().PadLeft(5, '0');
            // StockLog for Available Number in Stocks
            StockLog stockLog = new StockLog
            {
                ItemId = bhVM.ItemId,
                OldStock = latestLog.NewStock,
                NumberOfChange = bhVM.ReturnAmount,
                NewStock = latestLog.NewStock + bhVM.ReturnAmount,
                Remarks = StockLogTypeEnum.Returned.GetDisplayName(),
                RunningNo = runNo,
                LogNo = $"ST{logNoLeftPadded}",
                StockLogType = StockLogTypeEnum.Returned,
                CreatedDate = DateTime.Now,
                CreatedBy = itUser
            };
            _context.StockLogs.Add(stockLog);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ItemsController.Details), "Items", new { id = bhVM.ItemId });

        }


        // POST: BorrowHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowCreateViewModel bhVM)
        {
            if (ModelState.IsValid)
            {
                string itUser = Utility.GetCurrentUserName();
                BorrowHistory bh = new BorrowHistory
                {
                    ItemId = bhVM.ItemId,
                    RequestUser = bhVM.RequestUser,
                    RequestSectionId = bhVM.SelectedSectionId == 0 ? null : bhVM.SelectedSectionId,
                    RequestDepartmentId = bhVM.SelectedDepartmentId,
                    IsPermanentBorrow = bhVM.IsPermanentBorrow,
                    BorrowDate = bhVM.BorrowDate,
                    HasExpectedReturnDate = bhVM.HasExpectedReturnDate,
                    ExpectedReturnDate = bhVM.ExpectedReturnDate,
                    Itstaff = bhVM.Itstaff,
                    Amount = bhVM.Amount,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CreatedBy = itUser,
                    ModifiedBy = itUser,
                    IsDeleted = false
                };
                _context.BorrowHistories.Add(bh);

                Item? item = await _context.Items.FindAsync(bhVM.ItemId);
                if (item == null)
                {
                    return NotFound();
                }
                item.ItemStatus = ItemStatusEnum.Borrowed;
                item.AvailableAmount -= bhVM.Amount;
                item.BorrowedAmount += bhVM.Amount;
                item.ModifiedBy = itUser;
                item.ModifiedDate = DateTime.Now;

                StockLog? latestLog = await _context.StockLogs
                    .Where(sl => sl.ItemId == bhVM.ItemId)
                    .OrderByDescending(sl => sl.Id)
                    .FirstOrDefaultAsync();
                if (latestLog == null)
                {
                    return NotFound();
                }

                int runNo = latestLog.RunningNo + 1;
                string logNoLeftPadded = runNo.ToString().PadLeft(5, '0');
                StockLog stockLog = new StockLog
                {
                    ItemId = bhVM.ItemId,
                    OldStock = latestLog.NewStock,
                    NumberOfChange = (-1) * bhVM.Amount,
                    NewStock = latestLog.NewStock - bhVM.Amount,
                    Remarks = StockLogTypeEnum.Borrowed.GetDisplayName(),
                    RunningNo = runNo,
                    LogNo = $"ST{logNoLeftPadded}",
                    StockLogType = StockLogTypeEnum.Borrowed,
                    CreatedDate = DateTime.Now,
                    CreatedBy = itUser
                };
                _context.StockLogs.Add(stockLog);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ItemsController.Details), "Items", new { id = bhVM.ItemId });
            }
            return View(bhVM);
        }

        // GET: BorrowHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowHistory = await _context.BorrowHistories.FindAsync(id);
            if (borrowHistory == null)
            {
                return NotFound();
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", borrowHistory.ItemId);
            ViewData["RequestDepartmentId"] = new SelectList(_context.Departments, "Id", "Id", borrowHistory.RequestDepartmentId);
            ViewData["RequestSectionId"] = new SelectList(_context.Sections, "Id", "Id", borrowHistory.RequestSectionId);
            return View(borrowHistory);
        }

        // POST: BorrowHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ItemId,RequestUser,RequestSectionId,RequestDepartmentId,IsPermanentBorrow,BorrowDate,HasExpectedReturnDate,ExpectedReturnDate,IsReturn,ReturnDate,DurationDays,Itstaff,Amount,CreatedDate,ModifiedDate,CreatedBy,ModifiedBy,IsDeleted")] BorrowHistory borrowHistory)
        {
            if (id != borrowHistory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(borrowHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowHistoryExists(borrowHistory.Id))
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
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", borrowHistory.ItemId);
            ViewData["RequestDepartmentId"] = new SelectList(_context.Departments, "Id", "Id", borrowHistory.RequestDepartmentId);
            ViewData["RequestSectionId"] = new SelectList(_context.Sections, "Id", "Id", borrowHistory.RequestSectionId);
            return View(borrowHistory);
        }

        // GET: BorrowHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowHistory = await _context.BorrowHistories
                .Include(b => b.Item)
                .Include(b => b.RequestDepartment)
                .Include(b => b.RequestSection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrowHistory == null)
            {
                return NotFound();
            }

            return View(borrowHistory);
        }

        // POST: BorrowHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var borrowHistory = await _context.BorrowHistories.FindAsync(id);
            if (borrowHistory != null)
            {
                _context.BorrowHistories.Remove(borrowHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowHistoryExists(int id)
        {
            return _context.BorrowHistories.Any(e => e.Id == id);
        }

        private void PopulateDepartments(BorrowCreateViewModel model)
        {
            model.Departments = _context.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == model.SelectedDepartmentId
                }).ToList();

            model.Departments.Insert(0,
                new SelectListItem { Value = "", Text = "-- เลือกฝ่าย/กลุ่มงาน --" });
        }

        private void PopulateDepartments(ReturnCreateViewModel model)
        {
            model.Departments = _context.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == model.SelectedDepartmentId
                }).ToList();

            model.Departments.Insert(0,
                new SelectListItem { Value = "", Text = "-- เลือกฝ่าย/กลุ่มงาน --" });
        }

        private void PopulateSections(BorrowCreateViewModel model)
        {
            model.Sections = _context.Sections
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == model.SelectedSectionId
                }).ToList();

            model.Sections.Insert(0,
                //new SelectListItem { Value = "", Text = "-- เลือกหน่วยงาน --" });
                new SelectListItem { Value = 0.ToString(), Text = "-- เลือกหน่วยงาน --", Selected = 0 == model.SelectedSectionId });
        }

        private void PopulateSections(ReturnCreateViewModel model)
        {
            model.Sections = _context.Sections
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == model.SelectedSectionId
                }).ToList();

            model.Sections.Insert(0,
                //new SelectListItem { Value = "", Text = "-- เลือกหน่วยงาน --" });
                new SelectListItem { Value = 0.ToString(), Text = "-- เลือกหน่วยงาน --", Selected = 0 == model.SelectedSectionId });
        }
    }
}
