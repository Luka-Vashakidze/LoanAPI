using Project.Dtos;
using Project.Enums;
using Project.Models;

namespace Project.Services
{
    public interface IAccountantService
    {
        Task<bool> BlockUserAsync(int userId);
        Task<bool> UnblockUserAsync(int userId);
        Task<bool> ChangeLoanStatusAsync(int loanId, LoanStatus newStatus);
        Task<List<Loan>> GetAllLoansAsync();

        Task<bool> DeleteLoanAnyStatusAsync(int loanId);
        Task<bool> UpdateLoanAnyStatusAsync(int loanId, UpdateLoanDto request);
    }
}