using Project.Enums;

namespace Project.Dtos
{
    public class CreateLoanDto
    {
        public LoanType LoanType { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public int PeriodInMonths { get; set; }
    }
}