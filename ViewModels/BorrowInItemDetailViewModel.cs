namespace KlangIT_V3.ViewModels
{
    public class BorrowInItemDetailViewModel
    {
        public int Id { get; set; }
        public string RequestUser { get; set; } = null!;
        public string RequestDepartment { get; set; } = null!;
        public string? RequestSection { get; set; }
        public DateTime BorrowDate { get; set; }
        public string LatestITStaff { get; set; } = null!;
        public bool IsReturn { get; set; }
    }
}
