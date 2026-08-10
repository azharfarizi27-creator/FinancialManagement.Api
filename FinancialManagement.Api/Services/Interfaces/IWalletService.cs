using FinancialManagement.Api.DTOs.Wallet;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IWalletService
{
    Task<List<WalletResponse>> GetAllAsync(int userId);

    Task<WalletResponse?> GetByIdAsync(int id, int userId);

    Task<WalletResponse> CreateAsync(
        CreateWalletRequest request,
        int userId);

    Task<WalletResponse?> UpdateAsync(
        int id,
        UpdateWalletRequest request,
        int userId);

    Task<bool> DeleteAsync(int id, int userId);
}