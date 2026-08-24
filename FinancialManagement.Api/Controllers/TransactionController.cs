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

    public TransactionController(
        ITransactionService transactionService)
    {
        _transactionService = transactionService;
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