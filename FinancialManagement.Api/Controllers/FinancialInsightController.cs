using System.Security.Claims;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialInsightController : ControllerBase
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

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}