using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface IReservationRepository
    {
        ReservationResponse CreateReservation(ReservationRequest request);
        bool CancelReservation(Guid reservationId, Guid borrowerId);
        List<Reservation> GetReservationsByBorrower(Guid borrowerId);
        List<Reservation> GetReservationsByBook(Guid bookId);
        BookAvailabilityInfo GetBookAvailability(Guid bookId);
        Reservation? GetNextReservation(Guid bookId);
        bool UpdateReservationQueue(Guid bookId);
    }
}
