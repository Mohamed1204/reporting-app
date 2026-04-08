using ReportingApi1.Entities;

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

public class UpdateVatReportDto
{
    public ReportStatus Status { get; set; }
    public List<CreateSalesEntryDto> SalesEntries { get; set; } = new();
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
