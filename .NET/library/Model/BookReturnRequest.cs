namespace OneBeyondApi.Model
{
    public class BookReturnRequest
    {
        public Guid BookId { get; set; }
        public Guid BorrowerId { get; set; }
    }
}