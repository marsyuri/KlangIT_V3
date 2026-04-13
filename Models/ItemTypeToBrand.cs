using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class ItemTypeToBrand
{
    public int Id { get; set; }

    public int ItemTypeId { get; set; }

    public int ItemBrandId { get; set; }

    public virtual ItemBrand ItemBrand { get; set; } = null!;

    public virtual ItemType ItemType { get; set; } = null!;
}
