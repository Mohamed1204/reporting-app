namespace ReportingApi1.Entities;

public record TaxBreakdown(decimal VatAmount, decimal VatRate, VatScheme Scheme);
