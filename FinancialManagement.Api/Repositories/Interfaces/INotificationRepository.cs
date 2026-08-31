using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(int userId, int limit = 50);

    Task<Notification?> GetByIdAsync(int id, int userId);

    Task<int> GetUnreadCountAsync(int userId);

    Task<Notification> CreateAsync(Notification notification);

    Task UpdateAsync(Notification notification);

    Task MarkAllAsReadAsync(int userId);

    Task<bool> DeleteAsync(int id, int userId);

    Task ClearAllAsync(int userId);
}
