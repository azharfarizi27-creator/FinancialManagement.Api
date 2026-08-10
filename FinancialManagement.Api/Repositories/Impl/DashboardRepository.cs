using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
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
            .Include(transaction => transaction.Wallet)
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == userId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .Take(limit)
            .ToListAsync();
    }
}