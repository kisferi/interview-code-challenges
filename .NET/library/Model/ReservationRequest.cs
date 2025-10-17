namespace OneBeyondApi.Model
{
    public class ReservationRequest
    {
        public Guid BorrowerId { get; set; }
        public Guid BookId { get; set; }
    }
}
