using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationRepository _reservationRepository;

        public ReservationController(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        [HttpPost]
        [Route("Create")]
        public IActionResult CreateReservation([FromBody] ReservationRequest request)
        {
            if (request == null || request.BookId == Guid.Empty || request.BorrowerId == Guid.Empty)
            {
                return BadRequest("Invalid reservation request. BookId and BorrowerId are required.");
            }

            var response = _reservationRepository.CreateReservation(request);
            
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpDelete]
        [Route("{reservationId}/Cancel")]
        public IActionResult CancelReservation(Guid reservationId, [FromQuery] Guid borrowerId)
        {
            if (reservationId == Guid.Empty || borrowerId == Guid.Empty)
            {
                return BadRequest("Valid ReservationId and BorrowerId are required.");
            }

            bool success = _reservationRepository.CancelReservation(reservationId, borrowerId);
            
            if (success)
            {
                return Ok(new { message = "Reservation cancelled successfully." });
            }
            else
            {
                return NotFound("Reservation not found or not belonging to the specified borrower.");
            }
        }

        [HttpGet]
        [Route("Borrower/{borrowerId}")]
        public IActionResult GetBorrowerReservations(Guid borrowerId)
        {
            if (borrowerId == Guid.Empty)
            {
                return BadRequest("Valid BorrowerId is required.");
            }

            var reservations = _reservationRepository.GetReservationsByBorrower(borrowerId);
            return Ok(reservations);
        }

        [HttpGet]
        [Route("Book/{bookId}")]
        public IActionResult GetBookReservations(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                return BadRequest("Valid BookId is required.");
            }

            var reservations = _reservationRepository.GetReservationsByBook(bookId);
            return Ok(reservations);
        }

        [HttpGet]
        [Route("Book/{bookId}/Availability")]
        public IActionResult GetBookAvailability(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                return BadRequest("Valid BookId is required.");
            }

            var availability = _reservationRepository.GetBookAvailability(bookId);
            
            if (availability.BookId == Guid.Empty)
            {
                return NotFound("Book not found.");
            }

            return Ok(availability);
        }
    }
}
