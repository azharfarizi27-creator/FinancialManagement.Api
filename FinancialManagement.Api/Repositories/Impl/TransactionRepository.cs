using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetByUserIdAsync(int userId)
    {
        return await _context.Transactions
            .Where(transaction => transaction.UserId == userId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id, int userId)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(transaction =>
                transaction.Id == id &&
                transaction.UserId == userId);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);

        await _context.SaveChangesAsync();
    }
}