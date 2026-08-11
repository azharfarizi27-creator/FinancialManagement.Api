using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IBudgetRepository
{
    Task<List<Budget>> GetByUserIdAsync(int userId);

    Task<Budget?> GetByIdAsync(int id, int userId);

    Task<Budget?> GetByCategoryAndPeriodAsync(
        int userId,
        int categoryId,
        int month,
        int year);

    Task<Budget> CreateAsync(Budget budget);

    Task UpdateAsync(Budget budget);

    Task DeleteAsync(Budget budget);

    Task<decimal> GetUsedAmountAsync(
        int userId,
        int categoryId,
        int month,
        int year);
}