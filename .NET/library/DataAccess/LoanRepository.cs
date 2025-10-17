using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class LoanRepository : ILoanRepository
    {
        public LoanRepository()
        {
        }

        public List<BorrowerLoan> GetActiveLoans()
        {
            using (var context = new LibraryContext())
            {
                var activeLoans = context.Catalogue
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .Include(x => x.OnLoanTo)
                    .Where(x => x.OnLoanTo != null && (x.LoanEndDate == null || x.LoanEndDate >= DateTime.Today))
                    .ToList();

                // Group by borrower and create the response
                var borrowerLoans = activeLoans
                    .GroupBy(x => x.OnLoanTo!)
                    .Select(group => new BorrowerLoan
                    {
                        BorrowerId = group.Key.Id,
                        BorrowerName = group.Key.Name,
                        BorrowerEmail = group.Key.EmailAddress,
                        LoanedBooks = group.Select(loan => new LoanedBook
                        {
                            BookId = loan.Book.Id,
                            BookTitle = loan.Book.Name,
                            AuthorName = loan.Book.Author.Name,
                            LoanEndDate = loan.LoanEndDate
                        }).ToList()
                    })
                    .ToList();

                return borrowerLoans;
            }
        }
        
        public bool ReturnBook(BookReturnRequest returnRequest)
        {
            using (var context = new LibraryContext())
            {
                // Find the book stock entry that matches the book and borrower
                var bookStock = context.Catalogue
                    .Include(x => x.OnLoanTo)
                    .FirstOrDefault(x => x.Book.Id == returnRequest.BookId 
                                      && x.OnLoanTo != null 
                                      && x.OnLoanTo.Id == returnRequest.BorrowerId);

                if (bookStock == null)
                {
                    // Book not found or not on loan to this borrower
                    return false;
                }

                // Clear the loan information to mark the book as returned
                bookStock.OnLoanTo = null;
                bookStock.LoanEndDate = null;

                context.SaveChanges();
                return true;
            }
        }
    }
}
