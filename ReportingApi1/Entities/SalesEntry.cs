namespace ReportingApi1.Entities;

public class SalesEntry
{
    public int Id { get; set; }
    
    public int VatReportId { get; set; }
    public VatReport VatReport { get; set; } = null!;
    
    public string Country { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal VatRate { get; set; }
    
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
