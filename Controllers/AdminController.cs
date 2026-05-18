using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using KlangIT_V3.Services;
using Microsoft.AspNetCore.Mvc;

namespace KlangIT_V3.Controllers
{
    // TODO: เพิ่ม [Authorize(Roles = "Admin")] เมื่อ role-based auth พร้อมใช้
    public class AdminController : Controller
    {
        private readonly ItLptWarehouseContext _context;
        private readonly IDataIntegrityService _integrityService;

        public AdminController(ItLptWarehouseContext context, IDataIntegrityService integrityService)
        {
            _context = context;
            _integrityService = integrityService;
        }

        [HttpGet]
        public IActionResult BackfillStockLog() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BackfillStockLog(bool confirm)
        {
            if (!confirm)
            {
                ViewBag.Error = "กรุณาติ๊ก ยืนยันการดำเนินการ ก่อนกดรัน";
                return View();
            }

            string itUser = User.GetUsernameLocalPart();
            var (itemsProcessed, logsCreated) =
                await StockLogBackfillHelper.BackfillOpeningBalancesAsync(_context, itUser);

            ViewBag.Done           = true;
            ViewBag.ItemsProcessed = itemsProcessed;
            ViewBag.LogsCreated    = logsCreated;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DataIntegrityCheck()
        {
            var report = await _integrityService.RunChecksAsync();
            return View(report);
        }
    }
}
