using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Dtos;
using Project.Services;

namespace Project.Controllers
{
    [Authorize(Roles = "Accountant")] 
    [Route("api/[controller]")]
    [ApiController]
    public class AccountantController : ControllerBase
    {
        private readonly IAccountantService _accountantService;

        public AccountantController(IAccountantService accountantService)
        {
            _accountantService = accountantService;
        }

        [HttpPatch("block-user/{userId}")]
        public async Task<IActionResult> BlockUser(int userId)
        {
            if (await _accountantService.BlockUserAsync(userId))
            {
                return Ok("User blocked successfully.");
            }
            return NotFound("User not found.");
        }

        [HttpPatch("unblock-user/{userId}")]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            if (await _accountantService.UnblockUserAsync(userId))
            {
                return Ok("User unblocked successfully.");
            }
            return NotFound("User not found.");
        }

        [HttpPatch("loan/{loanId}/status")]
        public async Task<IActionResult> ChangeStatus(int loanId, ChangeLoanStatusDto request)
        {
            if (await _accountantService.ChangeLoanStatusAsync(loanId, request.NewStatus))
            {
                return Ok("Loan status updated.");
            }
            return NotFound("Loan not found.");
        }

        [HttpGet("all-loans")]
        public async Task<ActionResult<List<Models.Loan>>> GetAllLoans()
        {
            return Ok(await _accountantService.GetAllLoansAsync());
        }
        [HttpPut("loan/{loanId}")]
        public async Task<IActionResult> UpdateLoan(int loanId, UpdateLoanDto request)
        {
            if (await _accountantService.UpdateLoanAnyStatusAsync(loanId, request))
            {
                return Ok("Loan updated by Accountant.");
            }
            return NotFound("Loan not found.");
        }

        [HttpDelete("loan/{loanId}")]
        public async Task<IActionResult> DeleteLoan(int loanId)
        {
            if (await _accountantService.DeleteLoanAnyStatusAsync(loanId))
            {
                return Ok("Loan deleted by Accountant.");
            }
            return NotFound("Loan not found.");
        }
    }
}