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

        var result =
            await _dashboardService.GetSummaryAsync(userId);

        return Ok(result);
    }

    [HttpGet("recent-transactions")]
    public async Task<IActionResult> GetRecentTransactions(
        [FromQuery] int limit = 5)
    {
        var userId = GetUserId();

        var result =
            await _dashboardService.GetRecentTransactionsAsync(
                userId,
                limit);

        return Ok(result);
    }

    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary(
        [FromQuery] int month,
        [FromQuery] int year)
    {
        if (month < 1 || month > 12)
        {
            return BadRequest(new
            {
                message = "Month harus antara 1 sampai 12."
            });
        }

        if (year < 2000 || year > 2100)
        {
            return BadRequest(new
            {
                message = "Year tidak valid."
            });
        }

        var userId = GetUserId();

        var result =
            await _dashboardService.GetMonthlySummaryAsync(
                userId,
                month,
                year);

        return Ok(result);
    }

    [HttpGet("category-summary")]
    public async Task<IActionResult> GetCategorySummary(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] string type = "Expense")
    {
        if (month < 1 || month > 12)
        {
            return BadRequest(new
            {
                message = "Month harus antara 1 sampai 12."
            });
        }

        if (year < 2000 || year > 2100)
        {
            return BadRequest(new
            {
                message = "Year tidak valid."
            });
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            return BadRequest(new
            {
                message = "Type wajib diisi."
            });
        }

        var userId = GetUserId();

        var result =
            await _dashboardService.GetCategorySummaryAsync(
                userId,
                month,
                year,
                type);

        return Ok(result);
    }

    [HttpGet("wallet-summary")]
    public async Task<IActionResult> GetWalletSummary()
    {
        var userId = GetUserId();

        var result =
            await _dashboardService.GetWalletSummaryAsync(
                userId);

        return Ok(result);
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}