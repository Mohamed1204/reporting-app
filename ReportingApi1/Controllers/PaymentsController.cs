using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportingApi1.Entities;
using ReportingApi1.Services;
using ReportingApi1.DTOs;

namespace ReportingApi1.Controllers;

[Route("api/vat-reports/{reportId:int}/payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentsController(IPaymentService paymentService)
    { 
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<CreatePaymentResponseDto>> RecordPayment(
        int reportId,
        [FromBody] CreatePaymentDto request)
    {
        var result = await _paymentService.CreatePaymentAsync(reportId, request);
        return Ok(result);
    }
}
