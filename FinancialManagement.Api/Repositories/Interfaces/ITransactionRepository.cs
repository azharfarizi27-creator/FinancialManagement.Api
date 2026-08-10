using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetByUserIdAsync(int userId);

    Task<Transaction?> GetByIdAsync(int id, int userId);

    Task<Transaction> CreateAsync(Transaction transaction);

    Task UpdateAsync(Transaction transaction);

    Task DeleteAsync(Transaction transaction);
}