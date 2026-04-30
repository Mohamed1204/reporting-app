using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using ReportingApi1.Services;

namespace ReportingApi1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VatReportsController : ControllerBase
{
    private readonly IVatReportService _vatReportService;
    private readonly ICurrentUserService _currentUserService;

    public VatReportsController(IVatReportService vatReportService, ICurrentUserService currentUserService)
    {
        _vatReportService = vatReportService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VatReportListItemDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? companyId = null,
        [FromQuery] ReportStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var effectiveCompanyId = _currentUserService.IsAdmin
            ? companyId
            : _currentUserService.CompanyId;

        var reports = await _vatReportService.GetAllAsync(page, pageSize, effectiveCompanyId, status, sortBy, sortDir);
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VatReportDto>> GetById(int id)
    {
        var report = await _vatReportService.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        return Ok(report);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<VatReportDto>> Create(CreateVatReportDto dto)
    {
        var report = await _vatReportService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
    }

    [HttpPut]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(UpdateVatReportDto dto)
    {
        await _vatReportService.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        await _vatReportService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("save")]
    public async Task<ActionResult<VatReportDto>> Save(UpdateVatReportDto dto)
    {
        var report = await _vatReportService.SaveAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<VatReportDto>> Submit(UpdateVatReportDto dto)
    {
        var report = await _vatReportService.SubmitAsync(dto);
        return Ok(report);
    } 
}
