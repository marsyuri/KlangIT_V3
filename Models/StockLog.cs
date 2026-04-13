using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class StockLog
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int OldStock { get; set; }

    public int NumberOfChange { get; set; }

    public int NewStock { get; set; }

    public string? Remarks { get; set; }

    public int RunningNo { get; set; }

    public string LogNo { get; set; } = null!;

    public int StockLogTypeId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual Item Item { get; set; } = null!;
}
