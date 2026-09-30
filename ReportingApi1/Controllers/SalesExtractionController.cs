using Microsoft.AspNetCore.Mvc;
using ReportingApi1.DTOs;
using ReportingApi1.Services;

namespace ReportingApi1.Controllers;

// Shares the api/SalesEntries prefix with SalesEntriesController so the route
// matches the frontend contract (POST /api/SalesEntries/extract).
[Route("api/SalesEntries")]
[ApiController]
public class SalesExtractionController : ControllerBase
{
    private readonly ISalesExtractionService _salesExtractionService;
    public SalesExtractionController(ISalesExtractionService salesExtractionService)
    {
        _salesExtractionService = salesExtractionService;
    }

    [HttpPost("extract")]
    public async Task<ActionResult<string>> Extract(
        [FromBody] ExtractSalesEntryDto request,
        CancellationToken ct)
    {
        var result = await _salesExtractionService.ExtractAsync(request.Text, ct);
        return Ok(result);
    }
}
