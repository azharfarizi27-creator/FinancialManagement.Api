using FinancialManagement.Api.DTOs.Transaction;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TransactionController : BaseApiController
{
    private readonly ITransactionService _transactionService;
    private readonly IExportService _exportService;

    public TransactionController(
        ITransactionService transactionService,
        IExportService exportService)
    {
        _transactionService = transactionService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? type = null,
        int? categoryId = null,
        int? walletId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var userId = GetUserId();

        if (page < 1)
        {
            return BadRequest(new
            {
                message = "Page harus lebih besar dari 0."
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "PageSize harus berada di antara 1 dan 100."
            });
        }

        if (!string.IsNullOrEmpty(type) && type != "Income" && type != "Expense")
        {
            return BadRequest(new
            {
                message = "Type harus Income atau Expense."
            });
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest(new
            {
                message = "StartDate tidak boleh lebih besar dari EndDate."
            });
        }

        var transactions =
            await _transactionService.GetAllAsync(
                userId,
                page,
                pageSize,
                type,
                categoryId,
                walletId,
                startDate,
                endDate);

        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var transaction =
            await _transactionService.GetByIdAsync(
                id,
                userId);

        if (transaction == null)
        {
            return NotFound(new
            {
                message = "Transaction tidak ditemukan."
            });
        }

        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTransactionRequest request)
    {
        var userId = GetUserId();

        var transaction =
            await _transactionService.CreateAsync(
                request,
                userId);

        if (transaction == null)
        {
            return NotFound(new
            {
                message = "Wallet atau Category tidak ditemukan atau bukan milik user."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = transaction.Id },
            transaction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateTransactionRequest request)
    {
        var userId = GetUserId();

        var transaction =
            await _transactionService.UpdateAsync(
                id,
                request,
                userId);

        if (transaction == null)
        {
            return NotFound(new
            {
                message = "Transaction, Wallet, atau Category tidak ditemukan atau bukan milik user."
            });
        }

        return Ok(transaction);
    }

    [HttpPost("{id}/receipt")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadReceipt(int id, IFormFile file)
    {
        var userId = GetUserId();
        var updated = await _transactionService.UploadReceiptAsync(id, userId, file);

        if (updated == null)
        {
            return NotFound(new
            {
                message = "Transaction tidak ditemukan atau bukan milik user."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Nota transaksi berhasil diunggah.",
            receiptUrl = updated.ReceiptUrl,
            transaction = updated
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        [FromQuery] string? type = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? walletId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetUserId();
        var normalizedFormat = format.Trim().ToLowerInvariant();

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        if (normalizedFormat == "csv")
        {
            var bytes = await _exportService.ExportTransactionsCsvAsync(userId, startDate, endDate, type, categoryId, walletId);
            return File(bytes, "text/csv", $"transactions_{timestamp}.csv");
        }
        else if (normalizedFormat == "xlsx" || normalizedFormat == "excel")
        {
            var bytes = await _exportService.ExportTransactionsExcelAsync(userId, startDate, endDate, type, categoryId, walletId);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"transactions_{timestamp}.xlsx");
        }
        else if (normalizedFormat == "pdf")
        {
            var bytes = await _exportService.ExportTransactionsPdfAsync(userId, startDate, endDate, type, categoryId, walletId);
            return File(bytes, "application/pdf", $"transactions_{timestamp}.pdf");
        }

        return BadRequest(new
        {
            message = "Format export tidak didukung. Pilihan yang tersedia: csv, xlsx, pdf."
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted =
            await _transactionService.DeleteAsync(
                id,
                userId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Transaction tidak ditemukan."
            });
        }

        return Ok(new
        {
            message = "Transaction berhasil dihapus."
        });
    }
}