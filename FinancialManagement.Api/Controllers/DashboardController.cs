using System.Security.Claims;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();

        var summary =
            await _dashboardService.GetSummaryAsync(userId);

        return Ok(summary);
    }

    [HttpGet("recent-transactions")]
    public async Task<IActionResult> GetRecentTransactions(
        int limit = 5)
    {
        var userId = GetUserId();

        if (limit <= 0)
        {
            limit = 5;
        }

        var transactions =
            await _dashboardService.GetRecentTransactionsAsync(
                userId,
                limit);

        return Ok(transactions);
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}