using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project.Constants;
using Project.Data;
using Project.Dtos;
using Project.Exceptions;
using Project.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly LoanDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(LoanDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<User> RegisterAsync(UserRegisterDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Username,
                Email = request.Email,
                Age = request.Age,
                MonthlyIncome = request.MonthlyIncome,
                Role = Roles.User,
                PasswordHash = Convert.ToBase64String(passwordHash),
                PasswordSalt = Convert.ToBase64String(passwordSalt)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string> LoginAsync(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.Username);

            if (user == null)
                throw new BadRequestException("Invalid username or password.");

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new BadRequestException("Invalid username or password.");

            if (user.IsBlocked && user.BlockedUntil.HasValue && user.BlockedUntil.Value <= DateTime.UtcNow)
            {
                user.IsBlocked = false;
                user.BlockedUntil = null;
                await _context.SaveChangesAsync();
            }

            if (user.IsBlocked)
                throw new ForbiddenException("Your account is blocked.");

            return CreateToken(user);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var secretKey = _config.GetSection("JwtSettings:SecretKey").Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var salt = Convert.FromBase64String(storedSalt);
            var hash = Convert.FromBase64String(storedHash);

            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(hash);
            }
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.Loans).FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}