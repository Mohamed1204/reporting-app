namespace ReportingApi1.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    public ICollection<VatReport> VatReports { get; set; } = new List<VatReport>();
}
