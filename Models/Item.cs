using System;
using System.Collections.Generic;

namespace KlangIT_V3.Models;

public partial class Item
{
    public int Id { get; set; }

    public int OrderNo { get; set; }

    public bool IsBulk { get; set; }

    public string? AssetId { get; set; }

    public string? SerialNo { get; set; }

    public int? ItemTypeId { get; set; }

    public int? ItemBrandId { get; set; }

    public int? ItemModelId { get; set; }

    public string? ItemDescription { get; set; }

    public string? ItemImageUrl { get; set; }

    public int TotalAmount { get; set; }

    public int ActiveAmount { get; set; }

    public int AvailableAmount { get; set; }

    public int BorrowedAmount { get; set; }

    public int DamagedAmount { get; set; }

    public int MinimumAmount { get; set; }

    public int ItemStatusId { get; set; }

    public string? AssetId1 { get; set; }

    public string? AssetId2 { get; set; }

    public string? AssetId3 { get; set; }

    public string? AssetId4 { get; set; }

    public string? OtherAssetId { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<BorrowHistory> BorrowHistories { get; set; } = new List<BorrowHistory>();

    public virtual ItemBrand? ItemBrand { get; set; }

    public virtual ItemModel? ItemModel { get; set; }

    public virtual ItemType? ItemType { get; set; }

    public virtual ICollection<StockLog> StockLogs { get; set; } = new List<StockLog>();
}
