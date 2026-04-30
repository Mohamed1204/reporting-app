using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using ReportingApi1.Exceptions;
using ReportingApi1.Validation;

namespace ReportingApi1.Services;

public interface IVatReportService
{
    Task<PagedResult<VatReportListItemDto>> GetAllAsync(int page, int pageSize, int? companyId = null, ReportStatus? status = null, string? sortBy = null, string? sortDir = null);
    Task<VatReportDto?> GetByIdAsync(int id);
    Task<List<VatReportDto>> GetByCompanyAsync(int companyId);
    Task<VatReportDto> CreateAsync(CreateVatReportDto dto);
    Task UpdateAsync(UpdateVatReportDto dto);
    Task DeleteAsync(int id);
    Task<VatReportDto> SaveAsync(UpdateVatReportDto dto);
    Task<VatReportDto> SubmitAsync(UpdateVatReportDto dto);
}

public class VatReportService : IVatReportService
{
    private readonly VatReportingContext _context;
    private readonly ICurrentUserService _currentUserService;

    public VatReportService(VatReportingContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<VatReportListItemDto>> GetAllAsync(int page, int pageSize, int? companyId = null, ReportStatus? status = null, string? sortBy = null, string? sortDir = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.VatReports.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(vr => vr.CompanyId == companyId.Value);

        if (status.HasValue)
            query = query.Where(vr => vr.Status == status.Value);

        var totalCount = await query.CountAsync();

        var ordered = ApplySort(query, sortBy, sortDir);

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(vr => new VatReportListItemDto
            {
                Id = vr.Id,
                CompanyId = vr.CompanyId,
                CompanyName = vr.Company.Name,
                ReportingPeriodId = vr.ReportingPeriodId,
                StartDate = vr.ReportingPeriod.StartDate,
                EndDate = vr.ReportingPeriod.EndDate,
                SubmittedAt = vr.SubmittedAt,
                Status = vr.Status,
                TotalAmount = vr.SalesEntries.Sum(se => se.Amount),
                TotalVat = vr.SalesEntries.Sum(se => se.Amount * se.VatRate / 100),
                RowVersion = vr.RowVersion
            })
            .ToListAsync();

        return new PagedResult<VatReportListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
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
        var exists = await _context.VatReports.AnyAsync(vr =>
            vr.CompanyId == dto.CompanyId && vr.ReportingPeriodId == dto.ReportingPeriodId);

        if (exists)
            throw new ConflictException($"A VAT report already exists for company {dto.CompanyId} and period {dto.ReportingPeriodId}.");

        var vatReport = new VatReport
        {
            CompanyId = dto.CompanyId,
            ReportingPeriodId = dto.ReportingPeriodId,
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

    public async Task UpdateAsync(UpdateVatReportDto dto)
    {
        var vatReport = await _context.VatReports
            .Include(vr => vr.SalesEntries)
            .FirstOrDefaultAsync(vr => vr.CompanyId == dto.CompanyId
                                    && vr.ReportingPeriodId == dto.ReportingPeriodId);

        if (vatReport == null)
            throw new NotFoundException($"No VAT report found for company {dto.CompanyId} and period {dto.ReportingPeriodId}.");

        _context.SalesEntries.RemoveRange(vatReport.SalesEntries);

        vatReport.SalesEntries = dto.SalesEntries.Select(se => new SalesEntry
        {
            VatReportId = vatReport.Id,
            Country = CountryCodes.Normalize(se.Country),
            Amount = se.Amount,
            VatRate = se.VatRate
        }).ToList();

        _context.Entry(vatReport).Property(vr => vr.RowVersion).OriginalValue = dto.RowVersion;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var vatReport = await _context.VatReports.FindAsync(id);
        if (vatReport == null)
            throw new NotFoundException($"VAT report {id} not found.");

        _context.VatReports.Remove(vatReport);
        await _context.SaveChangesAsync();
    }

    public async Task<VatReportDto> SubmitAsync(UpdateVatReportDto dto)
    {
        var companyId = _currentUserService.ResolveCompanyId(dto.CompanyId);

        var vatReport = await _context.VatReports
            .Include(vr => vr.SalesEntries)
            .FirstOrDefaultAsync(vr => vr.CompanyId == companyId
                                    && vr.ReportingPeriodId == dto.ReportingPeriodId);

        if (vatReport == null)
            throw new NotFoundException($"No VAT report found for company {companyId} and period {dto.ReportingPeriodId}.");

        if (vatReport.Status != ReportStatus.Draft && vatReport.Status != ReportStatus.Rejected)
            throw new ConflictException("Only Draft or Rejected reports can be submitted.");

        _context.SalesEntries.RemoveRange(vatReport.SalesEntries);

        vatReport.Status = ReportStatus.Submitted;
        vatReport.SubmittedAt = DateTime.UtcNow;
        vatReport.SalesEntries = dto.SalesEntries.Select(se => new SalesEntry
        {
            Country = CountryCodes.Normalize(se.Country),
            Amount = se.Amount,
            VatRate = se.VatRate
        }).ToList();


        _context.Entry(vatReport).Property(vr => vr.RowVersion).OriginalValue = dto.RowVersion;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(vatReport.Id))!;
    }



    public async Task<VatReportDto> SaveAsync(UpdateVatReportDto dto)
    {
        var companyId = _currentUserService.ResolveCompanyId(dto.CompanyId);

        var vatReport = await _context.VatReports
            .Include(vr => vr.SalesEntries)
            .FirstOrDefaultAsync(vr => vr.CompanyId == companyId
                                    && vr.ReportingPeriodId == dto.ReportingPeriodId);

        if (vatReport == null)
            throw new NotFoundException($"No VAT report found for company {companyId} and period {dto.ReportingPeriodId}.");

        if (vatReport.Status == ReportStatus.Submitted)
            throw new ConflictException("Cannot save a submitted report.");

        _context.SalesEntries.RemoveRange(vatReport.SalesEntries);

        vatReport.SalesEntries = dto.SalesEntries.Select(se => new SalesEntry
        {
            VatReportId = vatReport.Id,
            Country = CountryCodes.Normalize(se.Country),
            Amount = se.Amount,
            VatRate = se.VatRate
        }).ToList();

        vatReport.Status = ReportStatus.Draft;

        _context.Entry(vatReport).Property(vr => vr.RowVersion).OriginalValue = dto.RowVersion;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(vatReport.Id))!;
    }

    private static IOrderedQueryable<VatReport> ApplySort(IQueryable<VatReport> query, string? sortBy, string? sortDir)
    {
        var desc = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<VatReport> primary = (sortBy?.ToLowerInvariant()) switch
        {
            "submittedat" => desc ? query.OrderByDescending(vr => vr.SubmittedAt) : query.OrderBy(vr => vr.SubmittedAt),
            "companyname" => desc ? query.OrderByDescending(vr => vr.Company.Name) : query.OrderBy(vr => vr.Company.Name),
            "status"      => desc ? query.OrderByDescending(vr => vr.Status)       : query.OrderBy(vr => vr.Status),
            "startdate"   => desc ? query.OrderByDescending(vr => vr.ReportingPeriod.StartDate) : query.OrderBy(vr => vr.ReportingPeriod.StartDate),
            _             => query.OrderByDescending(vr => vr.Id),
        };

        return primary.ThenBy(vr => vr.Id);
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
