using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Constants;
using Project.Dtos;
using Project.Services;
using System.Security.Claims;

namespace Project.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost]
        public async Task<ActionResult<Models.Loan>> CreateLoan(CreateLoanDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var loan = await _loanService.CreateLoanAsync(userId, request);
            return Ok(loan);
        }

        [HttpGet("my-loans")]
        public async Task<ActionResult<List<Models.Loan>>> GetAllLoans()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value!;

            var loans = await _loanService.GetAllLoansAsync(userId, userRole);
            return Ok(loans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Loan>> GetLoanById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value!;

            
            var loan = await _loanService.GetLoanByIdAsync(id);

            if (loan == null) return NotFound("Loan not found");

            if (userRole != Roles.Accountant && loan.UserId != userId)
            {
                return StatusCode(403, "Access Denied: You cannot view this loan.");
            }

            return Ok(loan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, UpdateLoanDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            await _loanService.UpdateLoanAsync(id, userId, request);
            return Ok("Loan updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            await _loanService.DeleteLoanAsync(id, userId);
            return Ok("Loan deleted successfully.");
        }
    }
}