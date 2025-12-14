using Project.Dtos;
using Project.Models;

namespace Project.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserRegisterDto request);
        Task<string> LoginAsync(UserLoginDto request);
        Task<bool> UserExistsAsync(string username);

        Task<User?> GetUserByIdAsync(int id);
    }
}