using System.Security.Cryptography;
using System.Text;
using Project.Constants;
using Project.Models;

namespace Project.Data
{
    public static class DataSeeder
    {
        public static void SeedAccountant(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();

            if (context.Users.Any(u => u.Role == Roles.Accountant)) return;

            using var hmac = new HMACSHA512();
            context.Users.Add(new User
            {
                FirstName = "Admin",
                LastName = "Accountant",
                UserName = "accountant",
                Email = "accountant@bank.com",
                Age = 30,
                MonthlyIncome = 0,
                Role = Roles.Accountant,
                PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes("Admin123!"))),
                PasswordSalt = Convert.ToBase64String(hmac.Key)
            });
            context.SaveChanges();
        }
    }
}
