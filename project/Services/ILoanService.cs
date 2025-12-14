using Project.Dtos;
using Project.Models;

namespace Project.Services
{
    public interface ILoanService
    {
        Task<Loan> CreateLoanAsync(int userId, CreateLoanDto request);

        Task<List<Loan>> GetAllLoansAsync(int userId, string userRole);

        Task<Loan?> GetLoanByIdAsync(int id);

        Task<string> UpdateLoanAsync(int id, int userId, UpdateLoanDto request);

        Task<string> DeleteLoanAsync(int id, int userId);
    }
}