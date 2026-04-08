namespace ReportingApi1.DTOs;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
