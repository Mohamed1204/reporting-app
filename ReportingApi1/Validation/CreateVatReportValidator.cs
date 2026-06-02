using FluentValidation;
using ReportingApi1.DTOs;

namespace ReportingApi1.Validation
{
    public class CreateVatReportValidator : AbstractValidator<CreateVatReportDto>
    {
        public CreateVatReportValidator()
        {
            RuleFor(x => x.CompanyId).GreaterThan(0);
            RuleFor(x => x.ReportingPeriodId).GreaterThan(0);
            RuleFor(x => x.SalesEntries)
                .Must(entries => entries
                    .Select(se => CountryCodes.Normalize(se.BuyerCountry))
                    .GroupBy(c => c)
                    .All(g => g.Count() == 1))
                .WithMessage("Duplicate country codes are not allowed.");
            RuleFor(x => x.SalesEntries)
                .Custom((entries, ctx) =>
                {
                    foreach (var result in SalesEntryValidation.ValidateOssScope(entries))
                        ctx.AddFailure(result.ErrorMessage);
                });
        }
    }
}
