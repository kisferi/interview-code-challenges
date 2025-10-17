namespace OneBeyondApi.Model
{
    public class ReservationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? ReservationId { get; set; }
        public int? QueuePosition { get; set; }
        public DateTime? EstimatedAvailableDate { get; set; }
    }
}
