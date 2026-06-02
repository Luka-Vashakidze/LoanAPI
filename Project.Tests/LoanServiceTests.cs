using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Constants;
using Project.Dtos;
using Project.Enums;
using Project.Exceptions;
using Project.Models;
using Project.Services;

namespace Project.Tests
{
    public class LoanServiceTests
    {
        private static User MakeUser(bool isBlocked = false, DateTime? blockedUntil = null)
        {
            return new User
            {
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
                Email = "test@example.com",
                Age = 25,
                MonthlyIncome = 1000,
                Role = Roles.User,
                IsBlocked = isBlocked,
                BlockedUntil = blockedUntil,
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
        }

        private static CreateLoanDto MakeCreateDto()
        {
            return new CreateLoanDto
            {
                LoanType = LoanType.Fast,
                Amount = 5000,
                Currency = Currency.GEL,
                PeriodInMonths = 12
            };
        }

        [Fact]
        public async Task CreateLoan_ValidRequest_CreatesLoanWithInProcessStatus()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new LoanService(context);
            var loan = await service.CreateLoanAsync(user.Id, MakeCreateDto());

            Assert.NotNull(loan);
            Assert.Equal(LoanStatus.InProcess, loan.Status);
            Assert.Equal(5000, loan.Amount);
            Assert.Equal(user.Id, loan.UserId);
        }

        [Fact]
        public async Task CreateLoan_BlockedUser_ThrowsForbidden()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser(isBlocked: true);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new LoanService(context);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => service.CreateLoanAsync(user.Id, MakeCreateDto()));
        }

        [Fact]
        public async Task CreateLoan_NonExistentUser_ThrowsNotFound()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var service = new LoanService(context);

            await Assert.ThrowsAsync<NotFoundException>(
                () => service.CreateLoanAsync(999, MakeCreateDto()));
        }

        [Fact]
        public async Task CreateLoan_ExpiredBlock_AllowsLoan()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser(isBlocked: true, blockedUntil: DateTime.UtcNow.AddDays(-1));
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new LoanService(context);

            var loan = await service.CreateLoanAsync(user.Id, MakeCreateDto());

            Assert.NotNull(loan);
            Assert.Equal(LoanStatus.InProcess, loan.Status);
        }

        [Fact]
        public async Task CreateLoan_ActiveTimedBlock_ThrowsForbidden()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser(isBlocked: true, blockedUntil: DateTime.UtcNow.AddDays(1));
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new LoanService(context);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => service.CreateLoanAsync(user.Id, MakeCreateDto()));
        }

        [Fact]
        public async Task UpdateLoan_InProcessLoanOwnedByUser_Succeeds()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                LoanType = LoanType.Fast,
                Amount = 1000,
                Currency = Currency.GEL,
                PeriodInMonths = 6,
                Status = LoanStatus.InProcess,
                UserId = user.Id
            };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var service = new LoanService(context);

            await service.UpdateLoanAsync(loan.Id, user.Id, new UpdateLoanDto
            {
                LoanType = LoanType.Auto,
                Amount = 2000,
                Currency = Currency.USD,
                PeriodInMonths = 12
            });

            var updated = await context.Loans.FindAsync(loan.Id);
            Assert.Equal(2000, updated!.Amount);
            Assert.Equal(LoanType.Auto, updated.LoanType);
        }

        [Fact]
        public async Task UpdateLoan_ApprovedLoan_ThrowsBadRequest()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                LoanType = LoanType.Fast,
                Amount = 1000,
                Currency = Currency.GEL,
                PeriodInMonths = 6,
                Status = LoanStatus.Approved,
                UserId = user.Id
            };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var service = new LoanService(context);

            await Assert.ThrowsAsync<BadRequestException>(
                () => service.UpdateLoanAsync(loan.Id, user.Id, new UpdateLoanDto
                {
                    LoanType = LoanType.Auto,
                    Amount = 2000,
                    Currency = Currency.USD,
                    PeriodInMonths = 12
                }));
        }

        [Fact]
        public async Task UpdateLoan_LoanOwnedByDifferentUser_ThrowsForbidden()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var owner = MakeUser();
            context.Users.Add(owner);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                LoanType = LoanType.Fast,
                Amount = 1000,
                Currency = Currency.GEL,
                PeriodInMonths = 6,
                Status = LoanStatus.InProcess,
                UserId = owner.Id
            };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var service = new LoanService(context);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => service.UpdateLoanAsync(loan.Id, owner.Id + 999, new UpdateLoanDto
                {
                    LoanType = LoanType.Auto,
                    Amount = 2000,
                    Currency = Currency.USD,
                    PeriodInMonths = 12
                }));
        }

        [Fact]
        public async Task DeleteLoan_ApprovedLoan_ThrowsBadRequest()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                LoanType = LoanType.Fast,
                Amount = 1000,
                Currency = Currency.GEL,
                PeriodInMonths = 6,
                Status = LoanStatus.Approved,
                UserId = user.Id
            };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var service = new LoanService(context);

            await Assert.ThrowsAsync<BadRequestException>(
                () => service.DeleteLoanAsync(loan.Id, user.Id));
        }
    }
}