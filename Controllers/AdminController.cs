using KlangIT_V3.Helpers;
using KlangIT_V3.Models;
using Microsoft.AspNetCore.Mvc;

namespace KlangIT_V3.Controllers
{
    // TODO: เพิ่ม [Authorize(Roles = "Admin")] เมื่อ role-based auth พร้อมใช้
    public class AdminController : Controller
    {
        private readonly ItLptWarehouseContext _context;

        public AdminController(ItLptWarehouseContext context)
        {
            _context = context;
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
    }
}
