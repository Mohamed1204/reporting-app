namespace ReportingApi1.Entities;

public class VatReport
{
    public int Id { get; set; }
    
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public int ReportingPeriodId { get; set; }
    public ReportingPeriod ReportingPeriod { get; set; } = null!;
    
    public DateTime SubmittedAt { get; set; }
    public ReportStatus Status { get; set; }
    
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    public ICollection<SalesEntry> SalesEntries { get; set; } = new List<SalesEntry>();
}

public enum ReportStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}
