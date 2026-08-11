using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _context;

    public BudgetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Budget>> GetByUserIdAsync(int userId)
    {
        return await _context.Budgets
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == userId)
            .OrderByDescending(budget => budget.Year)
            .ThenByDescending(budget => budget.Month)
            .ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(
        int id,
        int userId)
    {
        return await _context.Budgets
            .Include(budget => budget.Category)
            .FirstOrDefaultAsync(budget =>
                budget.Id == id &&
                budget.UserId == userId);
    }

    public async Task<Budget?> GetByCategoryAndPeriodAsync(
        int userId,
        int categoryId,
        int month,
        int year)
    {
        return await _context.Budgets
            .FirstOrDefaultAsync(budget =>
                budget.UserId == userId &&
                budget.CategoryId == categoryId &&
                budget.Month == month &&
                budget.Year == year);
    }

    public async Task<Budget> CreateAsync(Budget budget)
    {
        _context.Budgets.Add(budget);

        await _context.SaveChangesAsync();

        return budget;
    }

    public async Task UpdateAsync(Budget budget)
    {
        _context.Budgets.Update(budget);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Budget budget)
    {
        _context.Budgets.Remove(budget);

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetUsedAmountAsync(
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