using FinancialManagement.Api.DTOs.Wallet;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _repository;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository repository,
        ILogger<WalletService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<WalletResponse>> GetAllAsync(int userId)
    {
        _logger.LogInformation("Mengambil seluruh dompet untuk UserId: {UserId}", userId);

        var wallets = await _repository.GetByUserIdAsync(userId);

        _logger.LogInformation("Ditemukan {Count} dompet untuk UserId: {UserId}", wallets.Count, userId);

        return wallets
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<WalletResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mengambil dompet Id {WalletId} untuk UserId: {UserId}", id, userId);

        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            _logger.LogWarning("Dompet Id {WalletId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse> CreateAsync(
        CreateWalletRequest request,
        int userId)
    {
        _logger.LogInformation("Membuat dompet baru '{Name}' (Type: {Type}, Balance: {Balance}) untuk UserId: {UserId}",
            request.Name, request.Type, request.Balance, userId);

        var wallet = new Wallet
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Balance = request.Balance,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(wallet);

        _logger.LogInformation("Dompet berhasil dibuat dengan Id {WalletId} untuk UserId: {UserId}", wallet.Id, userId);

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse?> UpdateAsync(
        int id,
        UpdateWalletRequest request,
        int userId)
    {
        _logger.LogInformation("Memperbarui dompet Id {WalletId} untuk UserId: {UserId}", id, userId);

        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            _logger.LogWarning("Gagal memperbarui: Dompet Id {WalletId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        wallet.Name = request.Name.Trim();
        wallet.Type = request.Type.Trim();
        wallet.Balance = request.Balance;

        await _repository.UpdateAsync(wallet);

        _logger.LogInformation("Dompet Id {WalletId} berhasil diperbarui untuk UserId: {UserId}", id, userId);

        return MapToResponse(wallet);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mencoba menghapus dompet Id {WalletId} untuk UserId: {UserId}", id, userId);

        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            _logger.LogWarning("Gagal menghapus: Dompet Id {WalletId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return false;
        }

        await _repository.DeleteAsync(wallet);

        _logger.LogInformation("Dompet Id {WalletId} berhasil dihapus untuk UserId: {UserId}", id, userId);

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