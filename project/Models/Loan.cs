using Project.Enums;

namespace Project.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public LoanType LoanType { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public int PeriodInMonths { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.InProcess;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}