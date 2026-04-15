using KlangIT_V3.Models;
using KlangIT_V3.Models.Enums;

namespace KlangIT_V3.ViewModels
{
    public class ItemDetailsViewModel
    {
        public int Id { get; set; }

        public int OrderNo { get; set; }

        public bool IsBulk { get; set; }

        public string? AssetId { get; set; }

        public string? SerialNo { get; set; }

        public int? ItemTypeId { get; set; }
        public ItemType? ItemType { get; set; } = null!;

        public int? ItemBrandId { get; set; }
        public ItemBrand? ItemBrand { get; set; } = null!;

        public int? ItemModelId { get; set; }
        public ItemModel? ItemModel { get; set; } = null!;

        public string? ItemDescription { get; set; }

        public string? ItemImageUrl { get; set; }

        public int TotalAmount { get; set; }

        public int ActiveAmount { get; set; }

        public int AvailableAmount { get; set; }

        public int BorrowedAmount { get; set; }

        public int DamagedAmount { get; set; }

        public int DisposedAmount { get; set; }

        public int MinimumAmount { get; set; }

        public ItemStatusEnum ItemStatus { get; set; }

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

        public List<BorrowInItemDetailViewModel> BHinItemDetails { get; set; } = new List<BorrowInItemDetailViewModel>();
    }
}
