using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IGoalRepository
{
    Task<List<SavingsGoal>> GetByUserIdAsync(int userId);

    Task<SavingsGoal?> GetByIdAsync(int id, int userId);

    Task<SavingsGoal?> GetTrackedByIdAsync(int id, int userId);

    Task<SavingsGoal> CreateAsync(SavingsGoal goal);

    Task UpdateAsync(SavingsGoal goal);

    Task DeleteAsync(SavingsGoal goal);

    Task<SavingsGoalDeposit> AddDepositAsync(SavingsGoalDeposit deposit);
}
