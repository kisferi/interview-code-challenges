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
        private readonly IFineRepository _fineRepository;

        public LoanController(ILogger<LoanController> logger, ILoanRepository loanRepository, IFineRepository fineRepository)
        {
            _logger = logger;
            _loanRepository = loanRepository;
            _fineRepository = fineRepository;
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

            var response = _loanRepository.ReturnBook(returnRequest);
            
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return NotFound(response.Message);
            }
        }

        [HttpGet]
        [Route("Fines/{borrowerId}")]
        public IActionResult GetBorrowerFines(Guid borrowerId, [FromQuery] bool unpaidOnly = false)
        {
            if (borrowerId == Guid.Empty)
            {
                return BadRequest("Valid BorrowerId is required.");
            }

            var fines = unpaidOnly 
                ? _fineRepository.GetUnpaidFinesByBorrower(borrowerId)
                : _fineRepository.GetAllFinesByBorrower(borrowerId);

            return Ok(fines);
        }
    }
}
