using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class ReservationRepository : IReservationRepository
    {
        public ReservationResponse CreateReservation(ReservationRequest request)
        {
            using (var context = new LibraryContext())
            {
                // Check if the book exists
                var book = context.Books
                    .Include(b => b.Author)
                    .FirstOrDefault(b => b.Id == request.BookId);
                
                if (book == null)
                {
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "Book not found."
                    };
                }

                // Check if the borrower exists
                var borrower = context.Borrowers.FirstOrDefault(b => b.Id == request.BorrowerId);
                if (borrower == null)
                {
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "Borrower not found."
                    };
                }

                // Check if borrower already has an active reservation for this book
                var existingReservation = context.Reservations
                    .FirstOrDefault(r => r.BookId == request.BookId && 
                                   r.BorrowerId == request.BorrowerId && 
                                   r.IsActive);

                if (existingReservation != null)
                {
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "You already have an active reservation for this book.",
                        ReservationId = existingReservation.Id,
                        QueuePosition = existingReservation.QueuePosition
                    };
                }

                // Check if book is available
                var availableCopy = context.Catalogue
                    .FirstOrDefault(c => c.Book.Id == request.BookId && c.OnLoanTo == null);

                if (availableCopy != null)
                {
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "Book is currently available for immediate checkout. No reservation needed."
                    };
                }

                // Get the next position in the queue
                var currentQueueSize = context.Reservations
                    .Count(r => r.BookId == request.BookId && r.IsActive);

                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    BorrowerId = request.BorrowerId,
                    BookId = request.BookId,
                    ReservationDate = DateTime.Now,
                    QueuePosition = currentQueueSize + 1,
                    IsActive = true
                };

                context.Reservations.Add(reservation);
                context.SaveChanges();

                // Calculate estimated availability
                var estimatedDate = CalculateEstimatedAvailableDate(context, request.BookId, reservation.QueuePosition);

                return new ReservationResponse
                {
                    Success = true,
                    Message = "Reservation created successfully.",
                    ReservationId = reservation.Id,
                    QueuePosition = reservation.QueuePosition,
                    EstimatedAvailableDate = estimatedDate
                };
            }
        }

        public bool CancelReservation(Guid reservationId, Guid borrowerId)
        {
            using (var context = new LibraryContext())
            {
                var reservation = context.Reservations
                    .FirstOrDefault(r => r.Id == reservationId && 
                                   r.BorrowerId == borrowerId && 
                                   r.IsActive);

                if (reservation == null)
                {
                    return false;
                }

                reservation.IsActive = false;
                context.SaveChanges();

                // Update queue positions for remaining reservations
                UpdateReservationQueue(reservation.BookId);

                return true;
            }
        }

        public List<Reservation> GetReservationsByBorrower(Guid borrowerId)
        {
            using (var context = new LibraryContext())
            {
                return context.Reservations
                    .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                    .Where(r => r.BorrowerId == borrowerId && r.IsActive)
                    .OrderBy(r => r.QueuePosition)
                    .ToList();
            }
        }

        public List<Reservation> GetReservationsByBook(Guid bookId)
        {
            using (var context = new LibraryContext())
            {
                return context.Reservations
                    .Include(r => r.Borrower)
                    .Where(r => r.BookId == bookId && r.IsActive)
                    .OrderBy(r => r.QueuePosition)
                    .ToList();
            }
        }

        public BookAvailabilityInfo GetBookAvailability(Guid bookId)
        {
            using (var context = new LibraryContext())
            {
                var book = context.Books
                    .Include(b => b.Author)
                    .FirstOrDefault(b => b.Id == bookId);

                if (book == null)
                {
                    return new BookAvailabilityInfo();
                }

                var allCopies = context.Catalogue
                    .Include(c => c.OnLoanTo)
                    .Where(c => c.Book.Id == bookId)
                    .ToList();

                var availableCopies = allCopies.Where(c => c.OnLoanTo == null).ToList();
                var loanedCopies = allCopies.Where(c => c.OnLoanTo != null).ToList();

                var reservations = context.Reservations
                    .Include(r => r.Borrower)
                    .Where(r => r.BookId == bookId && r.IsActive)
                    .OrderBy(r => r.QueuePosition)
                    .ToList();

                var earliestReturnDate = loanedCopies
                    .Where(c => c.LoanEndDate.HasValue)
                    .Min(c => c.LoanEndDate);

                return new BookAvailabilityInfo
                {
                    BookId = bookId,
                    BookTitle = book.Name,
                    AuthorName = book.Author.Name,
                    IsAvailable = availableCopies.Any(),
                    CurrentLoanEndDate = earliestReturnDate,
                    TotalCopies = allCopies.Count,
                    AvailableCopies = availableCopies.Count,
                    ReservationCount = reservations.Count,
                    EstimatedAvailableDate = CalculateEstimatedAvailableDate(context, bookId, reservations.Count + 1),
                    ReservationQueue = reservations.Select(r => new ReservationInfo
                    {
                        ReservationId = r.Id,
                        BorrowerId = r.BorrowerId,
                        BorrowerName = r.Borrower.Name,
                        ReservationDate = r.ReservationDate,
                        QueuePosition = r.QueuePosition
                    }).ToList()
                };
            }
        }

        public Reservation? GetNextReservation(Guid bookId)
        {
            using (var context = new LibraryContext())
            {
                return context.Reservations
                    .Include(r => r.Borrower)
                    .Where(r => r.BookId == bookId && r.IsActive)
                    .OrderBy(r => r.QueuePosition)
                    .FirstOrDefault();
            }
        }

        public bool UpdateReservationQueue(Guid bookId)
        {
            using (var context = new LibraryContext())
            {
                var reservations = context.Reservations
                    .Where(r => r.BookId == bookId && r.IsActive)
                    .OrderBy(r => r.QueuePosition)
                    .ToList();

                for (int i = 0; i < reservations.Count; i++)
                {
                    reservations[i].QueuePosition = i + 1;
                }

                context.SaveChanges();
                return true;
            }
        }

        private DateTime? CalculateEstimatedAvailableDate(LibraryContext context, Guid bookId, int queuePosition)
        {
            // Get all loaned copies of this book
            var loanedCopies = context.Catalogue
                .Where(c => c.Book.Id == bookId && c.OnLoanTo != null)
                .OrderBy(c => c.LoanEndDate)
                .ToList();

            if (!loanedCopies.Any())
            {
                return DateTime.Today; // Available now
            }

            // Calculate when books will be available based on loan end dates
            // Assume each loan might be extended by 7 days on average
            var availabilityDates = loanedCopies
                .Where(c => c.LoanEndDate.HasValue)
                .Select(c => c.LoanEndDate!.Value.AddDays(7)) // Add buffer for potential extensions
                .OrderBy(d => d)
                .ToList();

            if (queuePosition <= availabilityDates.Count)
            {
                return availabilityDates[queuePosition - 1];
            }

            // If queue is longer than available dates, estimate based on average loan period
            var lastAvailableDate = availabilityDates.LastOrDefault();
            if (lastAvailableDate == default)
            {
                lastAvailableDate = DateTime.Today.AddDays(14); // Default 2-week loan period
            }

            // Add estimated time for remaining queue positions (assume 2 weeks per position)
            var additionalWeeks = (queuePosition - availabilityDates.Count) * 2;
            return lastAvailableDate.AddDays(additionalWeeks * 7);
        }
    }
}
