using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class BorrowHistory
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public int ItemId { get; set; }

    public string RequestUser { get; set; } = null!;

    public int RequestDepartmentId { get; set; }

    public int? RequestSectionId { get; set; }

    public bool IsPermanentBorrow { get; set; }

    public DateTime BorrowDate { get; set; }

    public bool HasExpectedReturnDate { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }

    public bool IsReturn { get; set; }

    public DateTime? ReturnDate { get; set; }

    public int? DurationDays { get; set; }

    public string Itstaff { get; set; } = null!;

    public int Amount { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Department RequestDepartment { get; set; } = null!;

    public virtual Section? RequestSection { get; set; }
}
