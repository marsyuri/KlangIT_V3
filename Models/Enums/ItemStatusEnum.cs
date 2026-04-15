using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.Models.Enums
{
    public enum ItemStatusEnum
    {
        [Display(Name = "ไม่ระบุสถานะ")]
        Undefined = 0,
        [Display(Name = "ใช้งานได้")]
        Available = 1,
        [Display(Name = "ถูกยืมใช้งาน")]
        Borrowed = 2,
        [Display(Name = "ชำรุด/เสื่อมสภาพ")]
        Damaged = 3,
        [Display(Name = "จำหน่ายแล้ว")]
        Disposed = 4
    }
}