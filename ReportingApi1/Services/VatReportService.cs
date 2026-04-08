using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;

namespace ReportingApi1.Services;

public interface IVatReportService
{
    Task<List<VatReportDto>> GetAllAsync();
    Task<VatReportDto?> GetByIdAsync(int id);
    Task<List<VatReportDto>> GetByCompanyAsync(int companyId);
    Task<VatReportDto> CreateAsync(CreateVatReportDto dto);
    Task<bool> UpdateAsync(int id, UpdateVatReportDto dto);
    Task<bool> DeleteAsync(int id);
}

public class VatReportService : IVatReportService
{
    private readonly VatReportingContext _context;

    public VatReportService(VatReportingContext context)
    {
        _context = context;
    }

    public async Task<List<VatReportDto>> GetAllAsync()
    {
        return await _context.VatReports
            .Include(vr => vr.Company)
            .Include(vr => vr.ReportingPeriod)
            .Include(vr => vr.SalesEntries)
            .Select(vr => MapToDto(vr))
            .ToListAsync();
    }

    public async Task<VatReportDto?> GetByIdAsync(int id)
    {
        var vatReport = await _context.VatReports
            .Include(vr => vr.Company)
            .Include(vr => vr.ReportingPeriod)
            .Include(vr => vr.SalesEntries)
            .FirstOrDefaultAsync(vr => vr.Id == id);

        return vatReport == null ? null : MapToDto(vatReport);
    }

    public async Task<List<VatReportDto>> GetByCompanyAsync(int companyId)
    {
        return await _context.VatReports
            .Include(vr => vr.Company)
            .Include(vr => vr.ReportingPeriod)
            .Include(vr => vr.SalesEntries)
            .Where(vr => vr.CompanyId == companyId)
            .Select(vr => MapToDto(vr))
            .ToListAsync();
    }

    public async Task<VatReportDto> CreateAsync(CreateVatReportDto dto)
    {
        var vatReport = new VatReport
        {
            CompanyId = dto.CompanyId,
            ReportingPeriodId = dto.ReportingPeriodId,
            SubmittedAt = DateTime.UtcNow,
            Status = ReportStatus.Draft,
            SalesEntries = dto.SalesEntries.Select(se => new SalesEntry
            {
                Country = se.Country,
                Amount = se.Amount,
                VatRate = se.VatRate
            }).ToList()
        };

        _context.VatReports.Add(vatReport);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(vatReport.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateVatReportDto dto)
    {
        var vatReport = await _context.VatReports
            .Include(vr => vr.SalesEntries)
            .FirstOrDefaultAsync(vr => vr.Id == id);

        if (vatReport == null) return false;

        vatReport.Status = dto.Status;
        
        _context.SalesEntries.RemoveRange(vatReport.SalesEntries);
        
        vatReport.SalesEntries = dto.SalesEntries.Select(se => new SalesEntry
        {
            VatReportId = id,
            Country = se.Country,
            Amount = se.Amount,
            VatRate = se.VatRate
        }).ToList();

        _context.Entry(vatReport).Property(vr => vr.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vatReport = await _context.VatReports.FindAsync(id);
        if (vatReport == null) return false;

        _context.VatReports.Remove(vatReport);
        await _context.SaveChangesAsync();
        return true;
    }

    private static VatReportDto MapToDto(VatReport vatReport)
    {
        var salesEntries = vatReport.SalesEntries.Select(se => new SalesEntryDto
        {
            Id = se.Id,
            Country = se.Country,
            Amount = se.Amount,
            VatRate = se.VatRate,
            VatAmount = se.Amount * se.VatRate / 100
        }).ToList();

        return new VatReportDto
        {
            Id = vatReport.Id,
            CompanyId = vatReport.CompanyId,
            CompanyName = vatReport.Company.Name,
            ReportingPeriodId = vatReport.ReportingPeriodId,
            StartDate = vatReport.ReportingPeriod.StartDate,
            EndDate = vatReport.ReportingPeriod.EndDate,
            SubmittedAt = vatReport.SubmittedAt,
            Status = vatReport.Status,
            SalesEntries = salesEntries,
            TotalAmount = salesEntries.Sum(se => se.Amount),
            TotalVat = salesEntries.Sum(se => se.VatAmount),
            RowVersion = vatReport.RowVersion
        };
    }
}
