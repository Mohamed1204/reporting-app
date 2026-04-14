using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using ReportingApi1.Services;

namespace ReportingApi1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VatReportsController : ControllerBase
{
    private readonly IVatReportService _vatReportService;

    public VatReportsController(IVatReportService vatReportService)
    {
        _vatReportService = vatReportService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<VatReportDto>>> GetAll(
        [FromQuery] int? companyId = null,
        [FromQuery] ReportStatus? status = null)
    {
        var reports = await _vatReportService.GetAllAsync(companyId, status);
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
    public async Task<ActionResult<VatReportDto>> Create(CreateVatReportDto dto)
    {
        try
        {
            var report = await _vatReportService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { error = "A VAT report already exists for this company and reporting period.", details = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateVatReportDto dto)
    {
        try
        {
            var result = await _vatReportService.UpdateAsync(dto);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { error = "The VAT report was modified by another user. Please refresh and try again." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _vatReportService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("save")]
    public async Task<ActionResult<VatReportDto>> Save(UpdateVatReportDto dto)
    {
        try
        {
            var report = await _vatReportService.SaveAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { error = "A VAT report already exists for this company and reporting period.", details = ex.Message });
        }
    }
}
