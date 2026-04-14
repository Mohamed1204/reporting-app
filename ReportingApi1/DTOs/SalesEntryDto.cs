using System.ComponentModel.DataAnnotations;
using ReportingApi1.Validation;

namespace ReportingApi1.DTOs;

public class SalesEntryDto
{
    public int Id { get; set; }
    public string Country { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
}

public class CreateSalesEntryDto
{
    [Required]
    [ValidCountryCode]
    public string Country { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal VatRate { get; set; }
}
