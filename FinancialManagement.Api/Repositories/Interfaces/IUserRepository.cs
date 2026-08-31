using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);

    Task<User?> GetTrackedByIdAsync(int id);

    Task<User?> GetByEmailAsync(string email);

    Task UpdateAsync(User user);
}
