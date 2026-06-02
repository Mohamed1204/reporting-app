using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ReportingApi1.Entities;
using ReportingApi1.Validation;

namespace ReportingApi1.DTOs;

public class SalesEntryDto
{
    public int Id { get; set; }
    public string BuyerCountry { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BuyerType BuyerType { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductCategory ProductCategory { get; set; }

    public bool BuyerHasValidVatNumber { get; set; }
    public DateOnly SaleDate { get; set; }
    public TaxBreakdown? Breakdown { get; set; }
}

public class CreateSalesEntryDto
{
    [Required]
    [ValidCountryCode]
    public string BuyerCountry { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BuyerType BuyerType { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductCategory ProductCategory { get; set; }

    public bool BuyerHasValidVatNumber { get; set; }

    public DateOnly SaleDate { get; set; }
}
