using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;

namespace ReportingApi1.Services;

public interface IReportingPeriodService
{
    Task<List<ReportingPeriodDto>> GetAllAsync();
    Task<ReportingPeriodDto?> GetByIdAsync(int id);
    Task<ReportingPeriodDto> CreateAsync(CreateReportingPeriodDto dto);
    Task<bool> DeleteAsync(int id);
}

public class ReportingPeriodService : IReportingPeriodService
{
    private readonly VatReportingContext _context;

    public ReportingPeriodService(VatReportingContext context)
    {
        _context = context;
    }

    public async Task<List<ReportingPeriodDto>> GetAllAsync()
    {
        return await _context.ReportingPeriods
            .Select(rp => new ReportingPeriodDto
            {
                Id = rp.Id,
                StartDate = rp.StartDate,
                EndDate = rp.EndDate,
                Status = rp.Status
            })
            .ToListAsync();
    }

    public async Task<ReportingPeriodDto?> GetByIdAsync(int id)
    {
        var period = await _context.ReportingPeriods.FindAsync(id);
        if (period == null) return null;

        return new ReportingPeriodDto
        {
            Id = period.Id,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            Status = period.Status
        };
    }

    public async Task<ReportingPeriodDto> CreateAsync(CreateReportingPeriodDto dto)
    {
        var period = new ReportingPeriod
        {
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = PeriodStatus.Open
        };

        _context.ReportingPeriods.Add(period);
        await _context.SaveChangesAsync();

        return new ReportingPeriodDto
        {
            Id = period.Id,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            Status = period.Status
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var period = await _context.ReportingPeriods.FindAsync(id);
        if (period == null) return false;

        _context.ReportingPeriods.Remove(period);
        await _context.SaveChangesAsync();
        return true;
    }
}
