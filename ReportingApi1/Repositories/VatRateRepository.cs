using ReportingApi1.Entities;
using ReportingApi1.Exceptions;

namespace ReportingApi1.Repositories;

public interface IVatRateRepository
{
    decimal GetRate(string country, ProductCategory productCategory, DateOnly salesDate);
}

public class VatRateRepository : IVatRateRepository
{
    // Destination-country VAT rates. Reduced rates are simplified to a single rate per country
    // (real VAT has several reduced/zero rates that vary by category and date); these are
    // illustrative and would come from SKAT's published tables in production. The salesDate
    // parameter is the snapshot key for future date-versioned rates; today only current rates exist.
    private static readonly IReadOnlyDictionary<string, (decimal Standard, decimal Reduced)> Rates =
        new Dictionary<string, (decimal, decimal)>(StringComparer.OrdinalIgnoreCase)
        {
            ["AT"] = (20m, 10m),
            ["BE"] = (21m, 6m),
            ["BG"] = (20m, 9m),
            ["HR"] = (25m, 5m),
            ["CY"] = (19m, 5m),
            ["CZ"] = (21m, 12m),
            ["DK"] = (25m, 25m),  // Denmark has no reduced rate
            ["EE"] = (22m, 9m),
            ["FI"] = (25.5m, 14m),
            ["FR"] = (20m, 5.5m),
            ["DE"] = (19m, 7m),
            ["GR"] = (24m, 6m),
            ["HU"] = (27m, 5m),
            ["IE"] = (23m, 9m),
            ["IT"] = (22m, 10m),
            ["LV"] = (21m, 12m),
            ["LT"] = (21m, 9m),
            ["LU"] = (17m, 8m),
            ["MT"] = (18m, 7m),
            ["NL"] = (21m, 9m),
            ["PL"] = (23m, 8m),
            ["PT"] = (23m, 6m),
            ["RO"] = (19m, 9m),
            ["SK"] = (23m, 10m),
            ["SI"] = (22m, 9.5m),
            ["ES"] = (21m, 10m),
            ["SE"] = (25m, 12m),
        };

    private static readonly HashSet<ProductCategory> ReducedRateCategories = new()
    {
        ProductCategory.Food,
        ProductCategory.Books,
        ProductCategory.Medicine
    };

    public decimal GetRate(string country, ProductCategory productCategory, DateOnly salesDate)
    {
        if (string.IsNullOrWhiteSpace(country) || !Rates.TryGetValue(country, out var rate))
            throw new BadRequestException($"No VAT rate is configured for country '{country}'.");

        return ReducedRateCategories.Contains(productCategory) ? rate.Reduced : rate.Standard;
    }
}
