using FinancialManagement.Api.DTOs.Budget;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetResponse>> GetAllAsync(int userId);

    Task<BudgetResponse?> GetByIdAsync(
        int id,
        int userId);

    Task<BudgetResponse?> CreateAsync(
        CreateBudgetRequest request,
        int userId);

    Task<BudgetResponse?> UpdateAsync(
        int id,
        UpdateBudgetRequest request,
        int userId);

    Task<bool> DeleteAsync(
        int id,
        int userId);
}