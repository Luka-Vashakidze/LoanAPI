using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Constants;
using Project.Enums;
using Project.Models;
using Project.Services;

namespace Project.Tests
{
    public class AccountantServiceTests
    {
        private static User MakeUser(string role = "User")
        {
            return new User
            {
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
                Email = "test@test.com",
                Age = 22,
                MonthlyIncome = 1200,
                Role = role,
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
        }

        [Fact]
        public async Task BlockUser_RegularUser_SetsBlockedFlagAndDate()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AccountantService(context);

            var result = await service.BlockUserAsync(user.Id, 7);

            Assert.True(result);
            var updated = await context.Users.FindAsync(user.Id);
            Assert.True(updated!.IsBlocked);
            Assert.NotNull(updated.BlockedUntil);
            Assert.True(updated.BlockedUntil > DateTime.UtcNow);
        }

        [Fact]
        public async Task BlockUser_AccountantTarget_IsRefused()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var accountant = MakeUser(role: Roles.Accountant);
            context.Users.Add(accountant);
            await context.SaveChangesAsync();

            var service = new AccountantService(context);
            var result = await service.BlockUserAsync(accountant.Id, 7);
            Assert.False(result);
            var unchanged = await context.Users.FindAsync(accountant.Id);
            Assert.False(unchanged!.IsBlocked);
            Assert.Null(unchanged.BlockedUntil);
        }

        [Fact]
        public async Task UnblockUser_ClearsBlockState()
        {
            using var context = TestHelpers.CreateInMemoryContext();
            var user = MakeUser();
            user.IsBlocked = true;
            user.BlockedUntil = DateTime.UtcNow.AddDays(5);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var service = new AccountantService(context);
            var result = await service.UnblockUserAsync(user.Id);

            Assert.True(result);
            var updated = await context.Users.FindAsync(user.Id);
            Assert.False(updated!.IsBlocked);
            Assert.Null(updated.BlockedUntil);
        }
    }
}