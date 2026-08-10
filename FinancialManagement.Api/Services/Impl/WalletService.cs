using FinancialManagement.Api.DTOs.Wallet;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _repository;

    public WalletService(IWalletRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WalletResponse>> GetAllAsync(int userId)
    {
        var wallets = await _repository.GetByUserIdAsync(userId);

        return wallets
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<WalletResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            return null;
        }

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse> CreateAsync(
        CreateWalletRequest request,
        int userId)
    {
        var wallet = new Wallet
        {
            UserId = userId,
            Name = request.Name,
            Type = request.Type,
            Balance = request.Balance,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(wallet);

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse?> UpdateAsync(
        int id,
        UpdateWalletRequest request,
        int userId)
    {
        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            return null;
        }

        wallet.Name = request.Name;
        wallet.Type = request.Type;
        wallet.Balance = request.Balance;

        await _repository.UpdateAsync(wallet);

        return MapToResponse(wallet);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            return false;
        }

        await _repository.DeleteAsync(wallet);

        return true;
    }

    private static WalletResponse MapToResponse(Wallet wallet)
    {
        return new WalletResponse
        {
            Id = wallet.Id,
            Name = wallet.Name,
            Type = wallet.Type,
            Balance = wallet.Balance,
            CreatedAt = wallet.CreatedAt
        };
    }
}