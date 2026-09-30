using FluentValidation;
using ReportingApi1.DTOs;

namespace ReportingApi1.Validation
{
    public class ExtractSalesEntryValidator : AbstractValidator<ExtractSalesEntryDto>
    {
        public ExtractSalesEntryValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty()
                .MaximumLength(10_000);
        }
    }
}
