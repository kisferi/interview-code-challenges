namespace OneBeyondApi.Model
{
    public class BookAvailabilityInfo
    {
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public DateTime? CurrentLoanEndDate { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int ReservationCount { get; set; }
        public DateTime? EstimatedAvailableDate { get; set; }
        public List<ReservationInfo> ReservationQueue { get; set; } = new List<ReservationInfo>();
    }

    public class ReservationInfo
    {
        public Guid ReservationId { get; set; }
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public int QueuePosition { get; set; }
    }
}
