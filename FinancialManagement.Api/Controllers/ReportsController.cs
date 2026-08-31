using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ReportsController(
        IReportService reportService,
        IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? walletId)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest(new
            {
                message = "StartDate tidak boleh lebih besar dari EndDate."
            });
        }

        var userId = GetUserId();
        var result = await _reportService.GetSummaryAsync(userId, startDate, endDate, walletId);
        return Ok(result);
    }

    [HttpGet("cashflow")]
    public async Task<IActionResult> GetCashflow(
        [FromQuery] string period = "monthly",
        [FromQuery] int? year = null,
        [FromQuery] int? walletId = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var userId = GetUserId();
        var result = await _reportService.GetCashflowTrendAsync(userId, period, targetYear, walletId);
        return Ok(result);
    }

    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown(
        [FromQuery] string type = "Expense",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? walletId = null)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest(new
            {
                message = "StartDate tidak boleh lebih besar dari EndDate."
            });
        }

        var userId = GetUserId();
        var result = await _reportService.GetCategoryBreakdownAsync(userId, type, startDate, endDate, walletId);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "pdf",
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] int? walletId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest(new
            {
                message = "StartDate tidak boleh lebih besar dari EndDate."
            });
        }

        var userId = GetUserId();
        var normalizedFormat = format.Trim().ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        if (normalizedFormat == "pdf")
        {
            var bytes = await _exportService.ExportFinancialReportPdfAsync(userId, month, year, walletId, startDate, endDate);
            return File(bytes, "application/pdf", $"financial_report_{timestamp}.pdf");
        }

        return BadRequest(new
        {
            message = "Format export laporan saat ini mendukung format: pdf."
        });
    }
}
