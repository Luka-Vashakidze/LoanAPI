using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Dtos;
using Project.Enums;
using Project.Models;

namespace Project.Services
{
    public class LoanService : ILoanService
    {
        private readonly LoanDbContext _context;

        public LoanService(LoanDbContext context)
        {
            _context = context;
        }

        public async Task<Loan> CreateLoanAsync(int userId, CreateLoanDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsBlocked)
            {
                throw new Exception("User is blocked or does not exist.");
            }

            var loan = new Loan
            {
                LoanType = request.LoanType,
                Amount = request.Amount,
                Currency = request.Currency,
                PeriodInMonths = request.PeriodInMonths,
                Status = LoanStatus.InProcess, 
                UserId = userId
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            return loan;
        }

        public async Task<List<Loan>> GetAllLoansAsync(int userId, string userRole)
        {
            if (userRole == "Accountant")
            {
                return await _context.Loans.Include(l => l.User).ToListAsync();
            }
            else
            {
                return await _context.Loans.Where(l => l.UserId == userId).ToListAsync();
            }
        }

        public async Task<Loan?> GetLoanByIdAsync(int id)
        {
            return await _context.Loans.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<string> UpdateLoanAsync(int id, int userId, UpdateLoanDto request)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null) return "Not Found";

            if (loan.UserId != userId) return "Access Denied";

            if (loan.Status != LoanStatus.InProcess) return "Cannot update loan that is not InProcess";

            loan.LoanType = request.LoanType;
            loan.Amount = request.Amount;
            loan.Currency = request.Currency;
            loan.PeriodInMonths = request.PeriodInMonths;

            await _context.SaveChangesAsync();
            return "Success";
        }

        public async Task<string> DeleteLoanAsync(int id, int userId)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null) return "Not Found";

            if (loan.UserId != userId) return "Access Denied";

            if (loan.Status != LoanStatus.InProcess) return "Cannot delete loan that is not InProcess";

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
            return "Success";
        }
    }
}