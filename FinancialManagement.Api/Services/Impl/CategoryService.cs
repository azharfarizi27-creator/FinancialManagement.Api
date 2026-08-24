using FinancialManagement.Api.DTOs.Category;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository repository,
        ILogger<CategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<CategoryResponse>> GetAllAsync(int userId)
    {
        _logger.LogInformation("Mengambil seluruh kategori untuk UserId: {UserId}", userId);

        var categories = await _repository.GetByUserIdAsync(userId);

        _logger.LogInformation("Ditemukan {Count} kategori untuk UserId: {UserId}", categories.Count, userId);

        return categories
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<CategoryResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mengambil kategori Id {CategoryId} untuk UserId: {UserId}", id, userId);

        var category = await _repository.GetByIdAsync(
            id,
            userId);

        if (category == null)
        {
            _logger.LogWarning("Kategori Id {CategoryId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        int userId)
    {
        _logger.LogInformation("Membuat kategori baru '{Name}' (Type: {Type}) untuk UserId: {UserId}",
            request.Name, request.Type, userId);

        var category = new Category
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type.Trim()
        };

        await _repository.CreateAsync(category);

        _logger.LogInformation("Kategori berhasil dibuat dengan Id {CategoryId} untuk UserId: {UserId}", category.Id, userId);

        return MapToResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        int userId)
    {
        _logger.LogInformation("Memperbarui kategori Id {CategoryId} untuk UserId: {UserId}", id, userId);

        var category = await _repository.GetByIdAsync(
            id,
            userId);

        if (category == null)
        {
            _logger.LogWarning("Gagal memperbarui: Kategori Id {CategoryId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        category.Name = request.Name.Trim();
        category.Type = request.Type.Trim();

        await _repository.UpdateAsync(category);

        _logger.LogInformation("Kategori Id {CategoryId} berhasil diperbarui untuk UserId: {UserId}", id, userId);

        return MapToResponse(category);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mencoba menghapus kategori Id {CategoryId} untuk UserId: {UserId}", id, userId);

        var category = await _repository.GetByIdAsync(
            id,
            userId);

        if (category == null)
        {
            _logger.LogWarning("Gagal menghapus: Kategori Id {CategoryId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return false;
        }

        await _repository.DeleteAsync(category);

        _logger.LogInformation("Kategori Id {CategoryId} berhasil dihapus untuk UserId: {UserId}", id, userId);

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