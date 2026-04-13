using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class Section
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public string Name { get; set; } = null!;

    public int DepartmentId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<BorrowHistory> BorrowHistories { get; set; } = new List<BorrowHistory>();

    public virtual Department Department { get; set; } = null!;
}
