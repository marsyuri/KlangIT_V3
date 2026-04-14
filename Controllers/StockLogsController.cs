using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KlangIT_V3.Models;

namespace KlangIT_V3.Controllers
{
    public class StockLogsController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public StockLogsController(ItLptWarehouseContext context)
        {
            _context = context;
        }

        // GET: StockLogs
        public async Task<IActionResult> Index()
        {
            var itLptWarehouseContext = _context.StockLogs.Include(s => s.Item);
            return View(await itLptWarehouseContext.ToListAsync());
        }

        // GET: StockLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockLog = await _context.StockLogs
                .Include(s => s.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stockLog == null)
            {
                return NotFound();
            }

            return View(stockLog);
        }

        // GET: StockLogs/Create
        public IActionResult Create()
        {
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id");
            //ViewData["StockLogType"] = new SelectList(_context.StockLogTypes, "Id", "Id");
            return View();
        }

        // POST: StockLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ItemId,OldStock,NumberOfChange,NewStock,Remarks,RunningNo,LogNo,StockLogType,CreatedDate,CreatedBy")] StockLog stockLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stockLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", stockLog.ItemId);
            //ViewData["StockLogType"] = new SelectList(_context.StockLogTypes, "Id", "Id", stockLog.StockLogType);
            return View(stockLog);
        }

        // GET: StockLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockLog = await _context.StockLogs.FindAsync(id);
            if (stockLog == null)
            {
                return NotFound();
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", stockLog.ItemId);
            //ViewData["StockLogType"] = new SelectList(_context.StockLogTypes, "Id", "Id", stockLog.StockLogType);
            return View(stockLog);
        }

        // POST: StockLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ItemId,OldStock,NumberOfChange,NewStock,Remarks,RunningNo,LogNo,StockLogType,CreatedDate,CreatedBy")] StockLog stockLog)
        {
            if (id != stockLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockLogExists(stockLog.Id))
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
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", stockLog.ItemId);
            //ViewData["StockLogType"] = new SelectList(_context.StockLogTypes, "Id", "Id", stockLog.StockLogType);
            return View(stockLog);
        }

        // GET: StockLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockLog = await _context.StockLogs
                .Include(s => s.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stockLog == null)
            {
                return NotFound();
            }

            return View(stockLog);
        }

        // POST: StockLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockLog = await _context.StockLogs.FindAsync(id);
            if (stockLog != null)
            {
                _context.StockLogs.Remove(stockLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockLogExists(int id)
        {
            return _context.StockLogs.Any(e => e.Id == id);
        }
    }
}
