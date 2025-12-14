using FluentValidation;
using Project.Dtos;
using Project.Enums;

namespace Project.Validators
{
    public class CreateLoanValidator : AbstractValidator<CreateLoanDto>
    {
        public CreateLoanValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Loan amount must be greater than 0.");

            RuleFor(x => x.PeriodInMonths)
                .InclusiveBetween(3, 60).WithMessage("Loan period must be between 3 and 60 months.");

            RuleFor(x => x.LoanType)
                .IsInEnum().WithMessage("Invalid loan type.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("Invalid currency.");
        }
    }
}