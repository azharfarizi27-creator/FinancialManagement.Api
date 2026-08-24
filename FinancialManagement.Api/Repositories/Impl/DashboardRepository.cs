using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Data;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetTotalBalanceAsync(int userId)
    {
        return await _context.Wallets
            .Where(wallet => wallet.UserId == userId)
            .SumAsync(wallet => wallet.Balance);
    }

    public async Task<decimal> GetTotalIncomeAsync(int userId)
    {
        return await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == "Income")
            .SumAsync(transaction => transaction.Amount);
    }

    public async Task<decimal> GetTotalExpenseAsync(int userId)
    {
        return await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == "Expense")
            .SumAsync(transaction => transaction.Amount);
    }

    public async Task<List<Transaction>> GetRecentTransactionsAsync(
        int userId,
        int limit = 5)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Wallet)
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == userId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<decimal> GetMonthlyIncomeAsync(
       int userId,
       int month,
       int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == "Income" &&
                transaction.TransactionDate >= startDate &&
                transaction.TransactionDate < endDate)
            .SumAsync(transaction => transaction.Amount);
    }

    public async Task<decimal> GetMonthlyExpenseAsync(
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

    // =========================
    // CATEGORY SUMMARY
    // =========================

    public async Task<List<CategorySummaryData>> GetCategorySummaryAsync(
        int userId,
        int month,
        int year,
        string type)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == type &&
                transaction.TransactionDate >= startDate &&
                transaction.TransactionDate < endDate)
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                transaction.Category.Name,
                transaction.Type
            })
            .Select(group => new CategorySummaryData
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.Name,
                TotalAmount = group.Sum(transaction => transaction.Amount),
                Type = group.Key.Type
            })
            .OrderByDescending(result => result.TotalAmount)
            .ToListAsync();
    }

    // =========================
    // WALLET SUMMARY
    // =========================

    public async Task<List<WalletSummaryData>> GetWalletSummaryAsync(
        int userId)
    {
        var wallets = await _context.Wallets
            .AsNoTracking()
            .Where(wallet => wallet.UserId == userId)
            .ToListAsync();

        var transactionSummary = await _context.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId)
            .GroupBy(transaction => transaction.WalletId)
            .Select(group => new
            {
                WalletId = group.Key,

                TotalIncome = group
                    .Where(transaction => transaction.Type == "Income")
                    .Sum(transaction => transaction.Amount),

                TotalExpense = group
                    .Where(transaction => transaction.Type == "Expense")
                    .Sum(transaction => transaction.Amount)
            })
            .ToListAsync();

        var result = wallets
            .Select(wallet =>
            {
                var summary = transactionSummary
                    .FirstOrDefault(x => x.WalletId == wallet.Id);

                var totalIncome = summary?.TotalIncome ?? 0;
                var totalExpense = summary?.TotalExpense ?? 0;

                return new WalletSummaryData
                {
                    WalletId = wallet.Id,
                    WalletName = wallet.Name,
                    Type = wallet.Type,
                    Balance = wallet.Balance,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    NetBalance = totalIncome - totalExpense
                };
            })
            .ToList();

        return result;
    }
}