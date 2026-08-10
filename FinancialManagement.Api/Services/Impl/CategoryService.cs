using FinancialManagement.Api.DTOs.Category;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoryResponse>> GetAllAsync(int userId)
    {
        var categories = await _repository.GetByUserIdAsync(userId);

        return categories
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<CategoryResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        var category = await _repository.GetByIdAsync(
            id,
            userId);

        if (category == null)
        {
            return null;
        }

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        int userId)
    {
        var category = new Category
        {
            UserId = userId,
            Name = request.Name,
            Type = request.Type
        };

        await _repository.CreateAsync(category);

        return MapToResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        int userId)
    {
        var category = await _repository.GetByIdAsync(
            id,
            userId);

        if (category == null)
        {
            return null;
        }

        category.Name = request.Name;
        category.Type = request.Type;

        await _repository.UpdateAsync(category);

        return MapToResponse(category);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var category = await _repository.GetByIdAsync(
            id,
            userId);

        if (category == null)
        {
            return false;
        }

        await _repository.DeleteAsync(category);

        return true;
    }

    private static CategoryResponse MapToResponse(
        Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type
        };
    }
}