using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> _logger;
        private readonly ILoanRepository _loanRepository;

        public LoanController(ILogger<LoanController> logger, ILoanRepository loanRepository)
        {
            _logger = logger;
            _loanRepository = loanRepository;
        }

        [HttpGet]
        [Route("OnLoan")]
        public IList<BorrowerLoan> GetActiveLoans()
        {
            return _loanRepository.GetActiveLoans();
        }

        [HttpPost]
        [Route("Return")]
        public IActionResult ReturnBook([FromBody] BookReturnRequest returnRequest)
        {
            if (returnRequest == null || returnRequest.BookId == Guid.Empty || returnRequest.BorrowerId == Guid.Empty)
            {
                return BadRequest("Invalid return request. BookId and BorrowerId are required.");
            }

            bool success = _loanRepository.ReturnBook(returnRequest);
            
            if (success)
            {
                return Ok(new { message = "Book returned successfully." });
            }
            else
            {
                return NotFound("Book not found or not currently on loan to the specified borrower.");
            }
        }
    }
}
