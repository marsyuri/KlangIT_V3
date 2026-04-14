using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.Models.Enums
{
    public enum StockTypeEnum
    {
        [Display(Name = "ไม่ระบุประเภท Stock")]
        Undefined = 0,
        [Display(Name = "เริ่มต้นข้อมูลวัสดุ")]
        Initial = 1,
        [Display(Name = "ยืมแล้ว")]
        Borrowed = 2,
        [Display(Name = "คืนแล้ว")]
        Returned = 3,
        [Display(Name = "แก้ไขข้อมูลวัสดุ")]
        Edited = 4,
        [Display(Name = "เสียหาย")]
        Damaged = 5,
        [Display(Name = "จำหน่ายแล้ว")]
        Disposed = 6
    }
}
