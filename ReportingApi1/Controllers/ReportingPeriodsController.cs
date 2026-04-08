using Microsoft.AspNetCore.Mvc;
using ReportingApi1.DTOs;
using ReportingApi1.Services;

namespace ReportingApi1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportingPeriodsController : ControllerBase
{
    private readonly IReportingPeriodService _periodService;

    public ReportingPeriodsController(IReportingPeriodService periodService)
    {
        _periodService = periodService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReportingPeriodDto>>> GetAll()
    {
        var periods = await _periodService.GetAllAsync();
        return Ok(periods);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReportingPeriodDto>> GetById(int id)
    {
        var period = await _periodService.GetByIdAsync(id);
        if (period == null)
            return NotFound();

        return Ok(period);
    }

    [HttpPost]
    public async Task<ActionResult<ReportingPeriodDto>> Create(CreateReportingPeriodDto dto)
    {
        var period = await _periodService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = period.Id }, period);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _periodService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
