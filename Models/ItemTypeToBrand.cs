using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class ItemTypeToBrand
{
    public int Id { get; set; }

    public int ItemTypeId { get; set; }

    public int ItemBrandId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string ModifiedBy { get; set; } = null!;

    public DateTime ModifiedDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ItemBrand ItemBrand { get; set; } = null!;

    public virtual ItemType ItemType { get; set; } = null!;
}
