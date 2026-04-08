using ReportingApi1.Entities;

namespace ReportingApi1.DTOs;

public class ReportingPeriodDto
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PeriodStatus Status { get; set; }
}

public class CreateReportingPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
