using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;

namespace ReportingApi1.Services;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<CompanyDto> CreateAsync(CreateCompanyDto dto);
    Task<bool> UpdateAsync(int id, UpdateCompanyDto dto);
    Task<bool> DeleteAsync(int id);
}

public class CompanyService : ICompanyService
{
    private readonly VatReportingContext _context;

    public CompanyService(VatReportingContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyDto>> GetAllAsync()
    {
        return await _context.Companies
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Country = c.Country
            })
            .ToListAsync();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return null;

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Country = company.Country
        };
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto)
    {
        var company = new Company
        {
            Name = dto.Name,
            Country = dto.Country
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Country = company.Country
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateCompanyDto dto)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return false;

        company.Name = dto.Name;
        company.Country = dto.Country;
        
        _context.Entry(company).Property(c => c.RowVersion).OriginalValue = dto.RowVersion;

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
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return false;

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
        return true;
    }
}
