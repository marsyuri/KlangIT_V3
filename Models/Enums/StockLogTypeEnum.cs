using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.Models.Enums
{
    public enum StockLogTypeEnum
    {
        [Display(Name = "รับเข้าเริ่มต้น (พร้อมใช้)")]
        InitialAvailable = 1,

        [Display(Name = "ย้อนหลัง — ถูกยืม")]
        InitialBorrowed = 2,

        [Display(Name = "ย้อนหลัง — ชำรุด")]
        InitialDamaged = 3,

        [Display(Name = "ย้อนหลัง — จำหน่ายแล้ว")]
        InitialDisposed = 9,

        [Display(Name = "ยืม")]
        Borrow = 4,

        [Display(Name = "คืน")]
        Return = 5,

        [Display(Name = "ชำรุด")]
        Damage = 6,

        [Display(Name = "จำหน่าย")]
        Dispose = 7,

        [Display(Name = "ปรับปรุงจำนวน")]
        Adjust = 8
    }
}
