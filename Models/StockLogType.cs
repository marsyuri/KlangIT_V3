using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class StockLogType
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<StockLog> StockLogs { get; set; } = new List<StockLog>();
}
