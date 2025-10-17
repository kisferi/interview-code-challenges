namespace OneBeyondApi.Model
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid BorrowerId { get; set; }
        public Borrower Borrower { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int QueuePosition { get; set; }
    }
}
