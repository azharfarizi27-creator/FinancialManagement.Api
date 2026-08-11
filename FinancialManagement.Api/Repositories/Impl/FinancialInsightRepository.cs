using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class FinancialInsightRepository : IFinancialInsightRepository
{
    private readonly AppDbContext _context;

    public FinancialInsightRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Budget>> GetActiveBudgetsAsync(int userId)
    {
        var now = DateTime.UtcNow;

        return await _context.Budgets
            .Include(budget => budget.Category)
            .Where(budget =>
                budget.UserId == userId &&
                budget.Month == now.Month &&
                budget.Year == now.Year)
            .ToListAsync();
    }

    public async Task<decimal> GetCurrentMonthIncomeAsync(
        int userId,
        int month,
        int year)
    {
        return await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == "Income" &&
                transaction.TransactionDate.Month == month &&
                transaction.TransactionDate.Year == year)
            .SumAsync(transaction => transaction.Amount);
    }

    public async Task<decimal> GetCurrentMonthExpenseAsync(
        int userId,
        int month,
        int year)
    {
        return await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == "Expense" &&
                transaction.TransactionDate.Month == month &&
                transaction.TransactionDate.Year == year)
            .SumAsync(transaction => transaction.Amount);
    }

    public async Task<decimal> GetPreviousMonthExpenseAsync(
        int userId,
        int month,
        int year)
    {
        var currentDate = new DateTime(year, month, 1);

        var previousDate = currentDate.AddMonths(-1);

        return await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == "Expense" &&
                transaction.TransactionDate.Month == previousDate.Month &&
                transaction.TransactionDate.Year == previousDate.Year)
            .SumAsync(transaction => transaction.Amount);
    }

    public async Task<List<Transaction>> GetCurrentMonthExpensesAsync(
        int userId,
        int month,
        int year)
    {
        return await _context.Transactions
            .Include(transaction => transaction.Category)
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == "Expense" &&
                transaction.TransactionDate.Month == month &&
                transaction.TransactionDate.Year == year)
            .OrderByDescending(transaction => transaction.Amount)
            .ToListAsync();
    }

    public async Task<decimal> GetCategoryMonthlyExpenseAsync(
    int userId,
    int categoryId,
    int month,
    int year)
    {
        return await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.CategoryId == categoryId &&
                transaction.Type == "Expense" &&
                transaction.TransactionDate.Month == month &&
                transaction.TransactionDate.Year == year)
            .SumAsync(transaction => transaction.Amount);
    }
}