using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetByUserIdAsync(int userId);

    Task<Transaction?> GetByIdAsync(int id, int userId);

    Task<Transaction> CreateAsync(Transaction transaction);

    Task UpdateAsync(Transaction transaction);

    Task DeleteAsync(Transaction transaction);

    Task<List<Transaction>> GetPagedByUserIdAsync(
        int userId,
        int skip,
        int take,
        string? type = null,
        int? categoryId = null,
        int? walletId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<int> CountByUserIdAsync(
        int userId,
        string? type = null,
        int? categoryId = null,
        int? walletId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
}