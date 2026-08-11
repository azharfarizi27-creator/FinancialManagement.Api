using FinancialManagement.Api.DTOs.Budget;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BudgetService(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<List<BudgetResponse>> GetAllAsync(int userId)
    {
        var budgets = await _budgetRepository.GetByUserIdAsync(userId);

        var responses = new List<BudgetResponse>();

        foreach (var budget in budgets)
        {
            responses.Add(await MapToResponse(budget, userId));
        }

        return responses;
    }

    public async Task<BudgetResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        var budget = await _budgetRepository.GetByIdAsync(
            id,
            userId);

        if (budget == null)
        {
            return null;
        }

        return await MapToResponse(budget, userId);
    }

    public async Task<BudgetResponse?> CreateAsync(
        CreateBudgetRequest request,
        int userId)
    {
        if (request.LimitAmount <= 0)
        {
            return null;
        }

        if (request.Month < 1 || request.Month > 12)
        {
            return null;
        }

        if (request.Year < 2000 || request.Year > 2100)
        {
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            return null;
        }

        var existingBudget =
            await _budgetRepository.GetByCategoryAndPeriodAsync(
                userId,
                request.CategoryId,
                request.Month,
                request.Year);

        if (existingBudget != null)
        {
            return null;
        }

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            LimitAmount = request.LimitAmount,
            Month = request.Month,
            Year = request.Year,
            CreatedAt = DateTime.UtcNow
        };

        await _budgetRepository.CreateAsync(budget);

        var createdBudget =
            await _budgetRepository.GetByIdAsync(
                budget.Id,
                userId);

        if (createdBudget == null)
        {
            return null;
        }

        return await MapToResponse(
            createdBudget,
            userId);
    }

    public async Task<BudgetResponse?> UpdateAsync(
        int id,
        UpdateBudgetRequest request,
        int userId)
    {
        if (request.LimitAmount <= 0)
        {
            return null;
        }

        if (request.Month < 1 || request.Month > 12)
        {
            return null;
        }

        if (request.Year < 2000 || request.Year > 2100)
        {
            return null;
        }

        var budget = await _budgetRepository.GetByIdAsync(
            id,
            userId);

        if (budget == null)
        {
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            return null;
        }

        var existingBudget =
            await _budgetRepository.GetByCategoryAndPeriodAsync(
                userId,
                request.CategoryId,
                request.Month,
                request.Year);

        if (existingBudget != null &&
            existingBudget.Id != id)
        {
            return null;
        }

        budget.CategoryId = request.CategoryId;
        budget.LimitAmount = request.LimitAmount;
        budget.Month = request.Month;
        budget.Year = request.Year;

        await _budgetRepository.UpdateAsync(budget);

        var updatedBudget =
            await _budgetRepository.GetByIdAsync(
                id,
                userId);

        if (updatedBudget == null)
        {
            return null;
        }

        return await MapToResponse(
            updatedBudget,
            userId);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var budget = await _budgetRepository.GetByIdAsync(
            id,
            userId);

        if (budget == null)
        {
            return false;
        }

        await _budgetRepository.DeleteAsync(budget);

        return true;
    }

    private async Task<BudgetResponse> MapToResponse(
        Budget budget,
        int userId)
    {
        var usedAmount =
            await _budgetRepository.GetUsedAmountAsync(
                userId,
                budget.CategoryId,
                budget.Month,
                budget.Year);

        return new BudgetResponse
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category.Name,
            LimitAmount = budget.LimitAmount,
            UsedAmount = usedAmount,
            RemainingAmount =
                budget.LimitAmount - usedAmount,
            Month = budget.Month,
            Year = budget.Year,
            CreatedAt = budget.CreatedAt
        };
    }
}