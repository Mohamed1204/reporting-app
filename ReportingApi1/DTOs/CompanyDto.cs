using ReportingApi1.Validation;
using System.ComponentModel.DataAnnotations;

namespace ReportingApi1.DTOs;

public class CompanyDto
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [ValidCountryCode, Required]
    public string Country { get; set; } = string.Empty;
}

public class CreateCompanyDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [ValidCountryCode]
    public string Country { get; set; } = string.Empty;
}

public class UpdateCompanyDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [ValidCountryCode]
    public string Country { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
