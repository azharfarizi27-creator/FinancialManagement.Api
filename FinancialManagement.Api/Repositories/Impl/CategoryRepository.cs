using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetByUserIdAsync(int userId)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id, int userId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(category =>
                category.Id == id &&
                category.UserId == userId);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
    }
}