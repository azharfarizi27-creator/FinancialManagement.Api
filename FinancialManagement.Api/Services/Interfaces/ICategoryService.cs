using FinancialManagement.Api.DTOs.Category;

namespace FinancialManagement.Api.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync(int userId);

    Task<CategoryResponse?> GetByIdAsync(int id, int userId);

    Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        int userId);

    Task<CategoryResponse?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        int userId);

    Task<bool> DeleteAsync(int id, int userId);
}