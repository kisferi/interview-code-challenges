namespace OneBeyondApi.Model
{
    public class BookReturnResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Fine? FineIssued { get; set; }
        public decimal? FineAmount { get; set; }
        public int? OverdueDays { get; set; }
    }
}
