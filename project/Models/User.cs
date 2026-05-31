using Project.Enums;
using System.Text.Json.Serialization;

namespace Project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public bool IsBlocked { get; set; } = false;
        public DateTime? BlockedUntil { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        [JsonIgnore]
        public string PasswordSalt { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}