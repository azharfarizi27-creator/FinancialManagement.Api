using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetByUserIdAsync(int userId);

    Task<Category?> GetByIdAsync(int id, int userId);

    Task<Category> CreateAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);
}