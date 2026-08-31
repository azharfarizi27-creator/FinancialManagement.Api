using FinancialManagement.Api.DTOs.Notification;

namespace FinancialManagement.Api.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetUserNotificationsAsync(int userId, int limit = 50);

    Task<UnreadNotificationCountResponse> GetUnreadCountAsync(int userId);

    Task<NotificationResponse> CreateNotificationAsync(int userId, CreateNotificationRequest request);

    Task<bool> MarkAsReadAsync(int id, int userId);

    Task<bool> MarkAllAsReadAsync(int userId);

    Task<bool> DeleteNotificationAsync(int id, int userId);

    Task<bool> ClearAllNotificationsAsync(int userId);
}
