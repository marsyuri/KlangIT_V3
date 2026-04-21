using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class StockLog
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int LogType { get; set; }

    public int DeltaAvailable { get; set; }

    public int DeltaBorrowed { get; set; }

    public int DeltaDamaged { get; set; }

    public int DeltaDisposed { get; set; }

    public int DeltaTotal { get; set; }

    public int TotalAfter { get; set; }

    public int AvailableAfter { get; set; }

    public int BorrowedAfter { get; set; }

    public int DamagedAfter { get; set; }

    public int DisposedAfter { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual Item Item { get; set; } = null!;
}
