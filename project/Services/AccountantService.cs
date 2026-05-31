using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Dtos;
using Project.Enums;
using Project.Models;

namespace Project.Services
{
    public class AccountantService : IAccountantService
    {
        private readonly LoanDbContext _context;

        public AccountantService(LoanDbContext context)
        {
            _context = context;
        }

        public async Task<bool> BlockUserAsync(int userId, int days)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsBlocked = true;
            user.BlockedUntil = DateTime.UtcNow.AddDays(days);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnblockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsBlocked = false;
            user.BlockedUntil = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeLoanStatusAsync(int loanId, LoanStatus newStatus)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null) return false;

            loan.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Loan>> GetAllLoansAsync()
        {
            return await _context.Loans.Include(l => l.User).ToListAsync();
        }
        public async Task<bool> DeleteLoanAnyStatusAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null) return false;

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLoanAnyStatusAsync(int loanId, UpdateLoanDto request)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null) return false;

            loan.Amount = request.Amount;
            loan.Currency = request.Currency;
            loan.LoanType = request.LoanType;
            loan.PeriodInMonths = request.PeriodInMonths;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}