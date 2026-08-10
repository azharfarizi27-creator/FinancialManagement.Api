using System.Security.Claims;
using FinancialManagement.Api.DTOs.Transaction;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(
        ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var transactions =
            await _transactionService.GetAllAsync(userId);

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
            return BadRequest(new
            {
                message =
                    "Wallet atau Category tidak ditemukan atau bukan milik user."
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
            return BadRequest(new
            {
                message =
                    "Transaction, Wallet, atau Category tidak ditemukan atau bukan milik user."
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

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}