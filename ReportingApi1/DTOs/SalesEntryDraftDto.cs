using System.Text.Json.Serialization;
using ReportingApi1.Entities;

namespace ReportingApi1.DTOs;

public class ExtractSalesEntryDto
{
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Suggested form values extracted from text, awaiting user review.
/// Null means missing or ambiguous; this is not a request to save a sales entry.
/// </summary>
public class SalesEntryDraftDto
{
    public string? BuyerCountry { get; set; }

    /// <summary>The amount before VAT, not the invoice's total including VAT.</summary>
    public decimal? Amount { get; set; }

    // Keep the extracted currency even if unsupported (e.g. USD), so review can
    // reject it instead of silently treating the amount as EUR.
    public string? Currency { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BuyerType? BuyerType { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductCategory? ProductCategory { get; set; }

    public DateOnly? SaleDate { get; set; }
}
