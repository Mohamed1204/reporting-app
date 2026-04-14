using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using ReportingApi1.Validation;

namespace ReportingApi1.Services;

public interface IVatReportService
{
    Task<List<VatReportDto>> GetAllAsync(int? companyId = null, ReportStatus? status = null);
    Task<VatReportDto?> GetByIdAsync(int id);
    Task<List<VatReportDto>> GetByCompanyAsync(int companyId);
    Task<VatReportDto> CreateAsync(CreateVatReportDto dto);
    Task<bool> UpdateAsync(UpdateVatReportDto dto);
    Task<bool> DeleteAsync(int id);
    Task<VatReportDto> SaveAsync(UpdateVatReportDto dto);
}

public class VatReportService : IVatReportService
{
    private readonly VatReportingContext _context;

    public VatReportService(VatReportingContext context)
    {
        _context = context;
    }

    public async Task<List<VatReportDto>> GetAllAsync(int? companyId = null, ReportStatus? status = null)
    {
        var query = _context.VatReports
            .Include(vr => vr.Company)
            .Include(vr => vr.ReportingPeriod)
            .Include(vr => vr.SalesEntries)
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(vr => vr.CompanyId == companyId.Value);

        if (status.HasValue)
            query = query.Where(vr => vr.Status == status.Value);

        return await query.Select(vr => MapToDto(vr)).ToListAsync();
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
                Country = CountryCodes.Normalize(se.Country),
                Amount = se.Amount,
                VatRate = se.VatRate
            }).ToList()
        };

        _context.VatReports.Add(vatReport);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(vatReport.Id))!;
    }

    public async Task<bool> UpdateAsync(UpdateVatReportDto dto)
    {
        var vatReport = await _context.VatReports
            .Include(vr => vr.SalesEntries)
            .FirstOrDefaultAsync(vr => vr.CompanyId == dto.CompanyId
                                    && vr.ReportingPeriodId == dto.ReportingPeriodId);

        if (vatReport == null) return false;

        vatReport.Status = ReportStatus.Submitted;
        
        _context.SalesEntries.RemoveRange(vatReport.SalesEntries);
        
        vatReport.SalesEntries = dto.SalesEntries.Select(se => new SalesEntry
        {
            VatReportId = vatReport.Id,
            Country = CountryCodes.Normalize(se.Country),
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

    public async Task<VatReportDto> SaveAsync(UpdateVatReportDto dto)
    {
        var vatReport = await _context.VatReports
            .Include(vr => vr.SalesEntries)
            .FirstOrDefaultAsync(vr => vr.CompanyId == dto.CompanyId
                                    && vr.ReportingPeriodId == dto.ReportingPeriodId);

        if (vatReport == null) return null!;
        if (vatReport.Status == ReportStatus.Submitted) return null!;

        _context.SalesEntries.RemoveRange(vatReport.SalesEntries);

        vatReport.SalesEntries = dto.SalesEntries.Select(se => new SalesEntry
        {
            VatReportId = vatReport.Id,
            Country = CountryCodes.Normalize(se.Country),
            Amount = se.Amount,
            VatRate = se.VatRate
        }).ToList();

        vatReport.Status = ReportStatus.Draft;
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(vatReport.Id))!;
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
