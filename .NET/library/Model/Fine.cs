namespace OneBeyondApi.Model
{
    public class Fine
    {
        public Guid Id { get; set; }
        public Guid BorrowerId { get; set; }
        public Borrower Borrower { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public decimal Amount { get; set; }
        public DateTime FineDate { get; set; }
        public DateTime DueDate { get; set; }
        public int OverdueDays { get; set; }
        public string Reason { get; set; } = "Late return";
        public bool IsPaid { get; set; } = false;
    }
}
