using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportingApi1.DTOs;
using ReportingApi1.Services;

namespace ReportingApi1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAll([FromQuery] string? name = null)
    {
        var companies = await _companyService.GetAllAsync(name);
        return Ok(companies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetById(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return NotFound();

        return Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(CreateCompanyDto dto)
    {
        var company = await _companyService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCompanyDto dto)
    {
        try
        {
            var result = await _companyService.UpdateAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { error = "The company was modified by another user. Please refresh and try again." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _companyService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
