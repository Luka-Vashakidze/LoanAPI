using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            if (userRole != "Accountant" && loan.UserId != userId)
            {
                return StatusCode(403, "Access Denied: You cannot view this loan.");
            }

            return Ok(loan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, UpdateLoanDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var result = await _loanService.UpdateLoanAsync(id, userId, request);

            if (result == "Not Found") return NotFound();
            if (result == "Access Denied") return StatusCode(403, "You cannot edit this loan.");
            if (result != "Success") return BadRequest(result);

            return Ok("Loan updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var result = await _loanService.DeleteLoanAsync(id, userId);

            if (result == "Not Found") return NotFound();
            if (result == "Access Denied") return StatusCode(403, "You cannot delete this loan.");
            if (result != "Success") return BadRequest(result);

            return Ok("Loan deleted successfully.");
        }
    }
}