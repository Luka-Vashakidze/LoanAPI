using Microsoft.EntityFrameworkCore;
using Project.Constants;
using Project.Data;
using Project.Dtos;
using Project.Enums;
using Project.Exceptions;
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
            if (user == null)
                throw new NotFoundException("User not found.");

            if (user.IsBlocked && user.BlockedUntil.HasValue && user.BlockedUntil.Value <= DateTime.UtcNow)
            {
                user.IsBlocked = false;
                user.BlockedUntil = null;
                await _context.SaveChangesAsync();
            }

            if (user.IsBlocked)
                throw new ForbiddenException("User is blocked and cannot request a loan.");

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
            if (userRole == Roles.Accountant)
            {
                return await _context.Loans.Include(l => l.User).ToListAsync();
            }

            return await _context.Loans.Where(l => l.UserId == userId).ToListAsync();
        }

        public async Task<Loan?> GetLoanByIdAsync(int id)
        {
            return await _context.Loans.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task UpdateLoanAsync(int id, int userId, UpdateLoanDto request)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
                throw new NotFoundException("Loan not found.");

            if (loan.UserId != userId)
                throw new ForbiddenException("You cannot edit this loan.");

            if (loan.Status != LoanStatus.InProcess)
                throw new BadRequestException("Loan can only be updated while it is InProcess.");

            loan.LoanType = request.LoanType;
            loan.Amount = request.Amount;
            loan.Currency = request.Currency;
            loan.PeriodInMonths = request.PeriodInMonths;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteLoanAsync(int id, int userId)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
                throw new NotFoundException("Loan not found.");

            if (loan.UserId != userId)
                throw new ForbiddenException("You cannot delete this loan.");

            if (loan.Status != LoanStatus.InProcess)
                throw new BadRequestException("Loan can only be deleted while it is InProcess.");

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
        }
    }
}