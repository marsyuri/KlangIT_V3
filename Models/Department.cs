using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class Department
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<BorrowHistory> BorrowHistories { get; set; } = new List<BorrowHistory>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
