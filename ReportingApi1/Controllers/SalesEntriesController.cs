using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ReportingApi1.DTOs;
using ReportingApi1.Services;

namespace ReportingApi1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesEntriesController : ControllerBase
    {
        private readonly SalesEntryService _salesEntryService;

        public SalesEntriesController(SalesEntryService salesEntryService)
        {
            _salesEntryService = salesEntryService;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SalesEntryDto>> DeleteById(int id)
        {
           var salesEntry = await _salesEntryService.DeleteByIdAsync(id);
            if (salesEntry == null) return NotFound();
             return Ok(salesEntry);
        }

    }
}
