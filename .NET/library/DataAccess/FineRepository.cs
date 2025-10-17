using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class FineRepository : IFineRepository
    {
        public Fine CreateFine(Guid borrowerId, Guid bookId, DateTime dueDate, int overdueDays, decimal amount)
        {
            using (var context = new LibraryContext())
            {
                var fine = new Fine
                {
                    Id = Guid.NewGuid(),
                    BorrowerId = borrowerId,
                    BookId = bookId,
                    Amount = amount,
                    FineDate = DateTime.Now,
                    DueDate = dueDate,
                    OverdueDays = overdueDays,
                    Reason = $"Late return - {overdueDays} day(s) overdue",
                    IsPaid = false
                };

                context.Fines.Add(fine);
                context.SaveChanges();
                return fine;
            }
        }

        public List<Fine> GetUnpaidFinesByBorrower(Guid borrowerId)
        {
            using (var context = new LibraryContext())
            {
                return context.Fines
                    .Include(f => f.Book)
                    .ThenInclude(b => b.Author)
                    .Where(f => f.BorrowerId == borrowerId && !f.IsPaid)
                    .OrderByDescending(f => f.FineDate)
                    .ToList();
            }
        }

        public List<Fine> GetAllFinesByBorrower(Guid borrowerId)
        {
            using (var context = new LibraryContext())
            {
                return context.Fines
                    .Include(f => f.Book)
                    .ThenInclude(b => b.Author)
                    .Where(f => f.BorrowerId == borrowerId)
                    .OrderByDescending(f => f.FineDate)
                    .ToList();
            }
        }
    }
}
