using System.ComponentModel.DataAnnotations;
using ReportingApi1.Entities;
using ReportingApi1.Validation;

namespace ReportingApi1.DTOs;

public class VatReportDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int ReportingPeriodId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmittedAt { get; set; }
    public ReportStatus Status { get; set; }
    public List<SalesEntryDto> SalesEntries { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public class VatReportListItemDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int ReportingPeriodId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmittedAt { get; set; }
    public ReportStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public class CreateVatReportDto
{
    public int CompanyId { get; set; }
    public int ReportingPeriodId { get; set; }
    public List<CreateSalesEntryDto> SalesEntries { get; set; } = new();
}

// Kept on the DA + IValidatableObject pattern as a reference example.
// CreateVatReportDto uses FluentValidation instead; don't mix both styles on the same DTO.
public class UpdateVatReportDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int CompanyId { get; set; }

    [Range(1, int.MaxValue)]
    public int ReportingPeriodId { get; set; }

    public ReportStatus Status { get; set; }
    public List<CreateSalesEntryDto> SalesEntries { get; set; } = new();
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        => SalesEntryValidation.ValidateNoDuplicateCountries(SalesEntries)
            .Concat(SalesEntryValidation.ValidateOssScope(SalesEntries));
}

internal static class SalesEntryValidation
{
    // Product categories that are VAT-exempt and therefore outside OSS scope.
    private static readonly HashSet<ProductCategory> ExemptCategories = new()
    {
        ProductCategory.FinancialServices,
        ProductCategory.Education
    };

    public static IEnumerable<ValidationResult> ValidateNoDuplicateCountries(
        List<CreateSalesEntryDto> salesEntries)
    {
        var duplicates = salesEntries
            .Select(se => CountryCodes.Normalize(se.BuyerCountry))
            .GroupBy(c => c)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            yield return new ValidationResult(
                $"Duplicate country codes are not allowed: {string.Join(", ", duplicates)}.",
                new[] { nameof(CreateVatReportDto.SalesEntries) });
        }
    }

    // Enforces the OSS scope boundary so out-of-scope sales never reach the VAT engine:
    // destination must be an EU member, B2B-with-valid-VAT-number is reverse-charge (not OSS),
    // and VAT-exempt categories are excluded.
    public static IEnumerable<ValidationResult> ValidateOssScope(
        List<CreateSalesEntryDto> salesEntries)
    {
        var member = new[] { nameof(CreateVatReportDto.SalesEntries) };

        foreach (var se in salesEntries)
        {
            if (!CountryCodes.IsEuMember(se.BuyerCountry))
                yield return new ValidationResult(
                    $"Buyer country '{se.BuyerCountry}' is not an EU member state and is outside OSS scope.",
                    member);

            if (se.BuyerType == BuyerType.B2B && se.BuyerHasValidVatNumber)
                yield return new ValidationResult(
                    $"B2B sales to a buyer with a valid VAT number ('{se.BuyerCountry}') are reverse-charge and outside OSS scope.",
                    member);

            if (ExemptCategories.Contains(se.ProductCategory))
                yield return new ValidationResult(
                    $"Product category '{se.ProductCategory}' is VAT-exempt and outside OSS scope.",
                    member);
        }
    }
}
