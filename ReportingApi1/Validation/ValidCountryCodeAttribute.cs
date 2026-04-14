using System.ComponentModel.DataAnnotations;

namespace ReportingApi1.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ValidCountryCodeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string code || !CountryCodes.IsValid(code))
        {
            return new ValidationResult(
                $"'{value}' is not a valid ISO 3166-1 alpha-2 country code.",
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}
