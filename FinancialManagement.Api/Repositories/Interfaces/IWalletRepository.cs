using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IWalletRepository
{
    Task<List<Wallet>> GetByUserIdAsync(int userId);

    Task<Wallet?> GetByIdAsync(int id, int userId);

    Task<Wallet> CreateAsync(Wallet wallet);

    Task UpdateAsync(Wallet wallet);

    Task DeleteAsync(Wallet wallet);
}