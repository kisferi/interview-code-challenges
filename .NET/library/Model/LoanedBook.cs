namespace OneBeyondApi.Model
{
    public class LoanedBook
    {
        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public DateTime? LoanEndDate { get; set; }
    }
}
