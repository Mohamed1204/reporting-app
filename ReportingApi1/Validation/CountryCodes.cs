namespace ReportingApi1.Validation;

/// <summary>
/// ISO 3166-1 alpha-2 country codes relevant for VAT reporting (EU + EEA + common).
/// </summary>
public static class CountryCodes
{
    private static readonly HashSet<string> EuMembers = new(StringComparer.OrdinalIgnoreCase)
    {
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
        "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL",
        "PL", "PT", "RO", "SK", "SI", "ES", "SE"
    };

    private static readonly HashSet<string> NonEuValidCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        // EEA
        "IS", "LI", "NO",
        // Other common
        "CH", "GB"
    };

    private static readonly HashSet<string> ValidCodes =
        new(EuMembers.Concat(NonEuValidCodes), StringComparer.OrdinalIgnoreCase);

    public static bool IsValid(string? code) =>
        !string.IsNullOrWhiteSpace(code) && ValidCodes.Contains(code);

    public static bool IsEuMember(string? code) =>
        !string.IsNullOrWhiteSpace(code) && EuMembers.Contains(code);

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();

    public static IReadOnlyCollection<string> All => ValidCodes;
}
