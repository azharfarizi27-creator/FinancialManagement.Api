using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class FinancialInsightController : BaseApiController
{
    private readonly IFinancialInsightService _service;

    public FinancialInsightController(
        IFinancialInsightService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetInsights()
    {
        var userId = GetUserId();

        var insights =
            await _service.GetInsightsAsync(userId);

        return Ok(insights);
    }
}