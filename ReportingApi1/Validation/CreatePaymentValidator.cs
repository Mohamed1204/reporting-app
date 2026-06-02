using FluentValidation;
using ReportingApi1.DTOs;

namespace ReportingApi1.Validation
{
    public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
    {
        public CreatePaymentValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.ExternalReferenceNumber)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
