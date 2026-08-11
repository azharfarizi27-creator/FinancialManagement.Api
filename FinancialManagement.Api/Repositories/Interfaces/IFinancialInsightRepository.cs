using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IFinancialInsightRepository
{
    Task<List<Budget>> GetActiveBudgetsAsync(int userId);

    Task<decimal> GetCurrentMonthIncomeAsync(
        int userId,
        int month,
        int year);

    Task<decimal> GetCurrentMonthExpenseAsync(
        int userId,
        int month,
        int year);

    Task<decimal> GetPreviousMonthExpenseAsync(
        int userId,
        int month,
        int year);

    Task<List<Transaction>> GetCurrentMonthExpensesAsync(
        int userId,
        int month,
        int year);

    Task<decimal> GetCategoryMonthlyExpenseAsync(
    int userId,
    int categoryId,
    int month,
    int year);
}