using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface IFineRepository
    {
        Fine CreateFine(Guid borrowerId, Guid bookId, DateTime dueDate, int overdueDays, decimal amount);
        List<Fine> GetUnpaidFinesByBorrower(Guid borrowerId);
        List<Fine> GetAllFinesByBorrower(Guid borrowerId);
    }
}
