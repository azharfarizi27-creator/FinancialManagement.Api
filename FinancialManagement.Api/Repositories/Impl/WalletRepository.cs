using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Wallet>> GetByUserIdAsync(int userId)
    {
        return await _context.Wallets
            .AsNoTracking()
            .Where(wallet => wallet.UserId == userId)
            .OrderByDescending(wallet => wallet.CreatedAt)
            .ToListAsync();
    }

    public async Task<Wallet?> GetByIdAsync(int id, int userId)
    {
        return await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(wallet =>
                wallet.Id == id &&
                wallet.UserId == userId);
    }

    public async Task<Wallet> CreateAsync(Wallet wallet)
    {
        _context.Wallets.Add(wallet);

        await _context.SaveChangesAsync();

        return wallet;
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        _context.Wallets.Update(wallet);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Wallet wallet)
    {
        _context.Wallets.Remove(wallet);

        await _context.SaveChangesAsync();
    }
}