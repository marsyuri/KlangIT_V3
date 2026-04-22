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
        InitialDisposed = 4,

        [Display(Name = "ยืม")]
        Borrow = 5,

        [Display(Name = "คืน")]
        Return = 6,

        [Display(Name = "ชำรุด")]
        Damage = 7,

        [Display(Name = "ซ่อมแล้ว")]
        Repair = 8,

        [Display(Name = "จำหน่าย")]
        Dispose = 9,

        [Display(Name = "ปรับปรุงจำนวน")]
        Adjust = 10,

        
    }
}
