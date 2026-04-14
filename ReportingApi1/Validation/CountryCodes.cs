namespace ReportingApi1.Validation;

/// <summary>
/// ISO 3166-1 alpha-2 country codes relevant for VAT reporting (EU + EEA + common).
/// </summary>
public static class CountryCodes
{
    private static readonly HashSet<string> ValidCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        // EU member states
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
        "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL",
        "PL", "PT", "RO", "SK", "SI", "ES", "SE",
        // EEA
        "IS", "LI", "NO",
        // Other common
        "CH", "GB"
    };

    public static bool IsValid(string? code) =>
        !string.IsNullOrWhiteSpace(code) && ValidCodes.Contains(code);

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();

    public static IReadOnlyCollection<string> All => ValidCodes;
}
