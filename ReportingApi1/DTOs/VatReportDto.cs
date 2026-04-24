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
        => SalesEntryValidation.ValidateNoDuplicateCountries(SalesEntries);
}

internal static class SalesEntryValidation
{
    public static IEnumerable<ValidationResult> ValidateNoDuplicateCountries(
        List<CreateSalesEntryDto> salesEntries)
    {
        var duplicates = salesEntries
            .Select(se => CountryCodes.Normalize(se.Country))
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
}
