using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Data;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<decimal> GetTotalBalanceAsync(int userId);

    Task<decimal> GetTotalIncomeAsync(int userId);

    Task<decimal> GetTotalExpenseAsync(int userId);

    Task<List<Transaction>> GetRecentTransactionsAsync(
        int userId,
        int limit = 5);

    Task<decimal> GetMonthlyIncomeAsync(
       int userId,
       int month,
       int year);

    Task<decimal> GetMonthlyExpenseAsync(
        int userId,
        int month,
        int year);

    // Category Summary
    Task<List<CategorySummaryData>> GetCategorySummaryAsync(
        int userId,
        int month,
        int year,
        string type);

    // Wallet Summary
    Task<List<WalletSummaryData>> GetWalletSummaryAsync(
        int userId);
}