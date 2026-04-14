using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class ItemModel
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public string Name { get; set; } = null!;

    public int ItemBrandId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ItemBrand ItemBrand { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
