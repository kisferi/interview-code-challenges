using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class LoanRepository : ILoanRepository
    {
        private readonly IFineRepository _fineRepository;
        private readonly IReservationRepository _reservationRepository;
        private const decimal DAILY_FINE_RATE = 0.50m; // $0.50 per day overdue

        public LoanRepository(IFineRepository fineRepository, IReservationRepository reservationRepository)
        {
            _fineRepository = fineRepository;
            _reservationRepository = reservationRepository;
        }

        // Parameterless constructor for backward compatibility
        public LoanRepository() : this(new FineRepository(), new ReservationRepository())
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
        
        public BookReturnResponse ReturnBook(BookReturnRequest returnRequest)
        {
            using (var context = new LibraryContext())
            {
                // Find the book stock entry that matches the book and borrower
                var bookStock = context.Catalogue
                    .Include(x => x.OnLoanTo)
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .FirstOrDefault(x => x.Book.Id == returnRequest.BookId 
                                      && x.OnLoanTo != null 
                                      && x.OnLoanTo.Id == returnRequest.BorrowerId);

                if (bookStock == null)
                {
                    return new BookReturnResponse
                    {
                        Success = false,
                        Message = "Book not found or not currently on loan to the specified borrower."
                    };
                }

                var response = new BookReturnResponse { Success = true };
                Fine? fineIssued = null;

                // Check if the book is being returned after the due date
                if (bookStock.LoanEndDate.HasValue && DateTime.Today > bookStock.LoanEndDate.Value)
                {
                    var overdueDays = (DateTime.Today - bookStock.LoanEndDate.Value).Days;
                    var fineAmount = overdueDays * DAILY_FINE_RATE;

                    // Create a fine for the overdue return
                    fineIssued = _fineRepository.CreateFine(
                        returnRequest.BorrowerId, 
                        returnRequest.BookId, 
                        bookStock.LoanEndDate.Value, 
                        overdueDays, 
                        fineAmount);

                    response.FineIssued = fineIssued;
                    response.FineAmount = fineAmount;
                    response.OverdueDays = overdueDays;
                    response.Message = $"Book returned successfully. Fine of ${fineAmount:F2} issued for {overdueDays} day(s) overdue.";
                }
                else
                {
                    response.Message = "Book returned successfully on time.";
                }

                // Clear the loan information to mark the book as returned
                bookStock.OnLoanTo = null;
                bookStock.LoanEndDate = null;

                context.SaveChanges();

                // Check if there are any reservations for this book
                var nextReservation = _reservationRepository.GetNextReservation(returnRequest.BookId);
                if (nextReservation != null)
                {
                    response.Message += $" The book is now reserved for {nextReservation.Borrower.Name}.";
                    response.NextReservation = new ReservationInfo
                    {
                        ReservationId = nextReservation.Id,
                        BorrowerId = nextReservation.BorrowerId,
                        BorrowerName = nextReservation.Borrower.Name,
                        ReservationDate = nextReservation.ReservationDate,
                        QueuePosition = nextReservation.QueuePosition
                    };
                }

                return response;
            }
        }
    }
}
