using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class ItemStatus
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
