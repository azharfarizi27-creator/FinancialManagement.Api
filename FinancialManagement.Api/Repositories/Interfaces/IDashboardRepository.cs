using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<decimal> GetTotalBalanceAsync(int userId);

    Task<decimal> GetTotalIncomeAsync(int userId);

    Task<decimal> GetTotalExpenseAsync(int userId);

    Task<List<Transaction>> GetRecentTransactionsAsync(
        int userId,
        int limit = 5);
}