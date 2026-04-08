namespace ReportingApi1.Entities;

public class ReportingPeriod
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PeriodStatus Status { get; set; }
    
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    public ICollection<VatReport> VatReports { get; set; } = new List<VatReport>();
}

public enum PeriodStatus
{
    Open,
    Closed,
    Locked
}
