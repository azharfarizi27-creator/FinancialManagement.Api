using FinancialManagement.Api.DTOs.Goal;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IGoalService
{
    Task<List<GoalResponse>> GetAllAsync(int userId);

    Task<GoalResponse?> GetByIdAsync(int id, int userId);

    Task<GoalResponse> CreateAsync(CreateGoalRequest request, int userId);

    Task<GoalResponse?> UpdateAsync(int id, UpdateGoalRequest request, int userId);

    Task<bool> DeleteAsync(int id, int userId);

    Task<GoalResponse> DepositAsync(int id, DepositGoalRequest request, int userId);
}
