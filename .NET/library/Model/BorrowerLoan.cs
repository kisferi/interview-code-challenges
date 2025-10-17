namespace OneBeyondApi.Model
{
    public class BorrowerLoan
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public string BorrowerEmail { get; set; }
        public List<LoanedBook> LoanedBooks { get; set; } = new List<LoanedBook>();
    }   
}
