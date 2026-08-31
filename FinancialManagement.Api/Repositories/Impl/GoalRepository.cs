using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class GoalRepository : IGoalRepository
{
    private readonly AppDbContext _context;

    public GoalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SavingsGoal>> GetByUserIdAsync(int userId)
    {
        return await _context.SavingsGoals
            .AsNoTracking()
            .Include(g => g.Deposits)
                .ThenInclude(d => d.Wallet)
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavingsGoal?> GetByIdAsync(int id, int userId)
    {
        return await _context.SavingsGoals
            .AsNoTracking()
            .Include(g => g.Deposits)
                .ThenInclude(d => d.Wallet)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    }

    public async Task<SavingsGoal?> GetTrackedByIdAsync(int id, int userId)
    {
        return await _context.SavingsGoals
            .Include(g => g.Deposits)
                .ThenInclude(d => d.Wallet)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    }

    public async Task<SavingsGoal> CreateAsync(SavingsGoal goal)
    {
        _context.SavingsGoals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task UpdateAsync(SavingsGoal goal)
    {
        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SavingsGoal goal)
    {
        _context.SavingsGoals.Remove(goal);
        await _context.SaveChangesAsync();
    }

    public async Task<SavingsGoalDeposit> AddDepositAsync(SavingsGoalDeposit deposit)
    {
        _context.SavingsGoalDeposits.Add(deposit);
        await _context.SaveChangesAsync();
        return deposit;
    }
}
