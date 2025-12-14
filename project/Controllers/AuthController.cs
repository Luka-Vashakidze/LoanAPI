using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Dtos;
using Project.Services; 
using System.Security.Claims;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Models.User>> Register(UserRegisterDto request)
        {
            if (await _authService.UserExistsAsync(request.Username))
            {
                return BadRequest("Username already exists.");
            }

            var user = await _authService.RegisterAsync(request);

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == "User not found")
            {
                return BadRequest("User not found.");
            }

            if (result == "Wrong password")
            {
                return BadRequest("Wrong password.");
            }

            if (result == "Blocked")
            {
                return StatusCode(403, "Your account is blocked.");
            }

            return Ok(new { token = result });
        }

        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<ActionResult<Models.User>> GetUser(int id)
        {
            
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value!;

            if (currentUserRole != "Accountant" && currentUserId != id)
            {
                return StatusCode(403, "Access Denied: You can only view your own profile.");
            }

            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found");

            return Ok(user);
        }
    }
}