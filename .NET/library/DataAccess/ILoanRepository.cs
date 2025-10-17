using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ILoanRepository
    {
        public List<BorrowerLoan> GetActiveLoans();
        public bool ReturnBook(BookReturnRequest returnRequest);
    }
}
