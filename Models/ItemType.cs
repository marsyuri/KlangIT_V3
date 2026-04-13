using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class ItemType
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<ItemTypeToBrand> ItemTypeToBrands { get; set; } = new List<ItemTypeToBrand>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
