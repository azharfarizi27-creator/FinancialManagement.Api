using FinancialManagement.Api.DTOs.Budget;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository,
        ILogger<BudgetService> logger)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<List<BudgetResponse>> GetAllAsync(int userId)
    {
        _logger.LogInformation("Mengambil seluruh anggaran (budgets) untuk UserId: {UserId}", userId);

        var budgets = await _budgetRepository.GetByUserIdAsync(userId);

        _logger.LogInformation("Ditemukan {Count} anggaran untuk UserId: {UserId}", budgets.Count, userId);

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
        _logger.LogInformation("Mengambil anggaran Id {BudgetId} untuk UserId: {UserId}", id, userId);

        var budget = await _budgetRepository.GetByIdAsync(
            id,
            userId);

        if (budget == null)
        {
            _logger.LogWarning("Anggaran Id {BudgetId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        return await MapToResponse(budget, userId);
    }

    public async Task<BudgetResponse?> CreateAsync(
        CreateBudgetRequest request,
        int userId)
    {
        _logger.LogInformation(
            "Membuat anggaran baru: UserId {UserId}, CategoryId {CategoryId}, LimitAmount {LimitAmount}, Month {Month}, Year {Year}",
            userId, request.CategoryId, request.LimitAmount, request.Month, request.Year);

        if (request.LimitAmount <= 0 || request.Month < 1 || request.Month > 12 || request.Year < 2000 || request.Year > 2100)
        {
            _logger.LogWarning("Gagal membuat anggaran: Parameter input tidak valid");
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            _logger.LogWarning("Gagal membuat anggaran: Kategori Id {CategoryId} tidak ditemukan untuk UserId {UserId}",
                request.CategoryId, userId);
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
            _logger.LogWarning("Gagal membuat anggaran: Anggaran untuk CategoryId {CategoryId} periode {Month}/{Year} sudah ada",
                request.CategoryId, request.Month, request.Year);
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
            _logger.LogError("Gagal memuat kembali anggaran yang baru dibuat dengan Id {BudgetId}", budget.Id);
            return null;
        }

        _logger.LogInformation("Anggaran berhasil dibuat dengan Id {BudgetId} untuk UserId: {UserId}", budget.Id, userId);

        return await MapToResponse(
            createdBudget,
            userId);
    }

    public async Task<BudgetResponse?> UpdateAsync(
        int id,
        UpdateBudgetRequest request,
        int userId)
    {
        _logger.LogInformation("Memperbarui anggaran Id {BudgetId} untuk UserId: {UserId}", id, userId);

        if (request.LimitAmount <= 0 || request.Month < 1 || request.Month > 12 || request.Year < 2000 || request.Year > 2100)
        {
            _logger.LogWarning("Gagal memperbarui anggaran: Parameter input tidak valid");
            return null;
        }

        var budget = await _budgetRepository.GetByIdAsync(
            id,
            userId);

        if (budget == null)
        {
            _logger.LogWarning("Gagal memperbarui: Anggaran Id {BudgetId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            _logger.LogWarning("Gagal memperbarui: Kategori Id {CategoryId} tidak ditemukan untuk UserId: {UserId}",
                request.CategoryId, userId);
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
            _logger.LogWarning("Gagal memperbarui: Konflik periode dengan anggaran Id {ExistingBudgetId}", existingBudget.Id);
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

        _logger.LogInformation("Anggaran Id {BudgetId} berhasil diperbarui untuk UserId: {UserId}", id, userId);

        return await MapToResponse(
            updatedBudget,
            userId);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mencoba menghapus anggaran Id {BudgetId} untuk UserId: {UserId}", id, userId);

        var budget = await _budgetRepository.GetByIdAsync(
            id,
            userId);

        if (budget == null)
        {
            _logger.LogWarning("Gagal menghapus: Anggaran Id {BudgetId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return false;
        }

        await _budgetRepository.DeleteAsync(budget);

        _logger.LogInformation("Anggaran Id {BudgetId} berhasil dihapus untuk UserId: {UserId}", id, userId);

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