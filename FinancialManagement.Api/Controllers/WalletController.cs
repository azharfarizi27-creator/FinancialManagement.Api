using FinancialManagement.Api.DTOs.Wallet;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class WalletController : BaseApiController
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var wallets = await _walletService.GetAllAsync(userId);

        return Ok(wallets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var wallet = await _walletService.GetByIdAsync(
            id,
            userId);

        if (wallet == null)
        {
            return NotFound(new
            {
                message = "Wallet tidak ditemukan."
            });
        }

        return Ok(wallet);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateWalletRequest request)
    {
        var userId = GetUserId();

        var wallet = await _walletService.CreateAsync(
            request,
            userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = wallet.Id },
            wallet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateWalletRequest request)
    {
        var userId = GetUserId();

        var wallet = await _walletService.UpdateAsync(
            id,
            request,
            userId);

        if (wallet == null)
        {
            return NotFound(new
            {
                message = "Wallet tidak ditemukan."
            });
        }

        return Ok(wallet);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted = await _walletService.DeleteAsync(
            id,
            userId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Wallet tidak ditemukan."
            });
        }

        return Ok(new
        {
            message = "Wallet berhasil dihapus."
        });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
        TransferWalletRequest request)
    {
        var userId = GetUserId();

        var result = await _walletService.TransferAsync(
            request,
            userId);

        return Ok(result);
    }
}