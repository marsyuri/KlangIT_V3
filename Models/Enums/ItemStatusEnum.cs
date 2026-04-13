using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.Models.Enums
{
    public enum ItemStatusEnum
    {
        [Display(Name = "ใช้งานได้")]
        Active = 1,
        [Display(Name = "ถูกใช้งาน")]
        Used = 2,
        [Display(Name = "ชำรุด/เสื่อมสภาพ")]
        Damaged = 3,
        [Display(Name = "รอจำหน่าย")]
        PendingDisposal = 4,
        [Display(Name = "จำหน่ายแล้ว")]
        Disposed = 5
    }
}