using KlangIT_V3.Models.Enums;

namespace KlangIT_V3.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับ Items/Delete
    /// Best Practice: ใช้ ViewModel แทน Model โดยตรง เพื่อ
    ///   1. ป้องกัน over-posting
    ///   2. ส่งข้อมูล dependency check ไปยัง View
    ///   3. ควบคุมว่า field ไหนแสดง/ซ่อน
    /// </summary>
    public class ItemDeleteViewModel
    {
        public int    Id            { get; set; }
        public string AssetId       { get; set; } = string.Empty;
        public string ItemTypeName  { get; set; } = string.Empty;
        public string ItemBrandName { get; set; } = string.Empty;
        public string ItemModelName { get; set; } = string.Empty;
        public int    AvailableAmount { get; set; }
        public ItemStatusEnum ItemStatus { get; set; }

        // ── Dependency check ──
        public int  BorrowHistoryCount   { get; set; }
        public bool HasActiveBorrow      { get; set; }   // มีรายการที่ยังไม่คืน
        public bool CanDelete            => BorrowHistoryCount == 0;
    }

    /// <summary>ViewModel สำหรับ ItemModels/Delete</summary>
    public class ItemModelDeleteViewModel
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = string.Empty;
        public string BrandName   { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy  { get; set; } = string.Empty;

        // ── Dependency check: มี Item ที่ใช้รุ่นนี้หรือไม่ ──
        public int  ItemCount  { get; set; }
        public bool CanDelete  => ItemCount == 0;
    }

    /// <summary>ViewModel สำหรับ ItemBrands/Delete</summary>
    public class ItemBrandDeleteViewModel
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }

        // ── Dependency check: มี ItemModel หรือ Item ที่ใช้ยี่ห้อนี้ ──
        public int  ItemModelCount { get; set; }
        public int  ItemCount      { get; set; }
        public bool CanDelete      => ItemModelCount == 0 && ItemCount == 0;
    }

    /// <summary>ViewModel สำหรับ ItemTypes/Delete</summary>
    public class ItemTypeDeleteViewModel
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }

        // ── Dependency check: มี Item ที่ใช้ประเภทนี้ ──
        public int  ItemCount  { get; set; }
        public bool CanDelete  => ItemCount == 0;
    }

    /// <summary>ViewModel สำหรับ Sections/Delete</summary>
    public class SectionDeleteViewModel
    {
        public int    Id             { get; set; }
        public string Name           { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;

        // ── Dependency check: มี BorrowHistory ที่อ้างอิง Section นี้ ──
        public int  BorrowHistoryCount { get; set; }
        public bool HasDependency      => BorrowHistoryCount > 0;
        public bool CanDelete          => true;   // Section ลบได้เสมอ แต่มี warning
    }

    /// <summary>ViewModel สำหรับ Departments/Delete</summary>
    public class DepartmentDeleteViewModel
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = string.Empty;

        // ── Dependency check ──
        public int  SectionCount       { get; set; }
        public int  BorrowHistoryCount { get; set; }
        public bool HasDependency      => SectionCount > 0 || BorrowHistoryCount > 0;
        public bool CanDelete          => true;   // soft delete
    }

    /// <summary>ViewModel สำหรับ BorrowHistories/Delete</summary>
    public class BorrowHistoryDeleteViewModel
    {
        public int    Id             { get; set; }
        public string ItemHeader     { get; set; } = string.Empty;
        public string ItemAssetId    { get; set; } = string.Empty;
        public string RequestUser    { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string SectionName    { get; set; } = string.Empty;
        public DateTime BorrowDate   { get; set; }
        public bool   IsReturn       { get; set; }
        public int    ItemId         { get; set; }

        // ลบ BorrowHistory ได้เสมอ (ไม่มี child table)
        public bool CanDelete => true;
    }

    /// <summary>ViewModel สำหรับ ItemTypeToBrands/Delete</summary>
    public class ItemTypeToBrandDeleteViewModel
    {
        public int    Id            { get; set; }
        public string ItemTypeName  { get; set; } = string.Empty;
        public string ItemBrandName { get; set; } = string.Empty;
        public bool   CanDelete     => true;
    }
}
