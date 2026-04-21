using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class BorrowHistory
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public int ItemId { get; set; }

    public string BorrowerUser { get; set; } = null!;

    public int BorrowerDepartmentId { get; set; }

    public int? BorrowerSectionId { get; set; }

    public string? BorrowTel { get; set; }

    public bool IsPermanentBorrow { get; set; }

    public bool IsInitial { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string BorrowItname { get; set; } = null!;

    public DateTime? ReturnDate { get; set; }

    public string ReturnItname { get; set; } = null!;

    public int Amount { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual Department BorrowerDepartment { get; set; } = null!;

    public virtual Section? BorrowerSection { get; set; }

    public virtual Item Item { get; set; } = null!;
}
