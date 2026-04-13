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
            Item item = await _context.Items.SingleOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
            {
                return NotFound();
            }

            BorrowHistoryViewModel viewModel = new BorrowHistoryViewModel
            {
                ItemId = item.Id,
                ItemAssetId = item.AssetId == null ? string.Empty : item.AssetId,
                BorrowDate = DateTime.Today,
                ExpectedReturnDate = DateTime.Today
            };
            PopulateDepartments(viewModel);
            PopulateSections(viewModel);
            return View(viewModel);
        }

        // POST: BorrowHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowHistoryViewModel bhVM)
        {
            if (ModelState.IsValid)
            {
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
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CreatedBy = Utility.GetCurrentUserName(),
                    ModifiedBy = Utility.GetCurrentUserName(),
                    IsDeleted = false
                };
                _context.BorrowHistories.Add(bh);
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

        private void PopulateDepartments(BorrowHistoryViewModel model)
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

        private void PopulateSections(BorrowHistoryViewModel model)
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
