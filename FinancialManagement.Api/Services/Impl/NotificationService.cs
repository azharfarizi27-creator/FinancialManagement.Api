using FinancialManagement.Api.DTOs.Notification;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<List<NotificationResponse>> GetUserNotificationsAsync(int userId, int limit = 50)
    {
        _logger.LogInformation("Mengambil notifikasi untuk UserId {UserId}", userId);

        var notifications = await _notificationRepository.GetByUserIdAsync(userId, limit);
        return notifications.Select(MapToResponse).ToList();
    }

    public async Task<UnreadNotificationCountResponse> GetUnreadCountAsync(int userId)
    {
        _logger.LogInformation("Menghitung notifikasi belum dibaca untuk UserId {UserId}", userId);

        var count = await _notificationRepository.GetUnreadCountAsync(userId);
        return new UnreadNotificationCountResponse
        {
            UnreadCount = count
        };
    }

    public async Task<NotificationResponse> CreateNotificationAsync(int userId, CreateNotificationRequest request)
    {
        _logger.LogInformation("Membuat notifikasi untuk UserId {UserId}: {Title}", userId, request.Title);

        var notification = new Notification
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Type = string.IsNullOrWhiteSpace(request.Type) ? "info" : request.Type.Trim().ToLower(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _notificationRepository.CreateAsync(notification);
        return MapToResponse(created);
    }

    public async Task<bool> MarkAsReadAsync(int id, int userId)
    {
        _logger.LogInformation("Menandai notifikasi Id {NotificationId} telah dibaca untuk UserId {UserId}", id, userId);

        var notification = await _notificationRepository.GetByIdAsync(id, userId);
        if (notification == null)
        {
            throw new NotFoundException("Notifikasi tidak ditemukan.");
        }

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        _logger.LogInformation("Menandai semua notifikasi telah dibaca untuk UserId {UserId}", userId);

        await _notificationRepository.MarkAllAsReadAsync(userId);
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int id, int userId)
    {
        _logger.LogInformation("Menghapus notifikasi Id {NotificationId} untuk UserId {UserId}", id, userId);

        var deleted = await _notificationRepository.DeleteAsync(id, userId);
        if (!deleted)
        {
            throw new NotFoundException("Notifikasi tidak ditemukan.");
        }

        return true;
    }

    public async Task<bool> ClearAllNotificationsAsync(int userId)
    {
        _logger.LogInformation("Menghapus semua notifikasi untuk UserId {UserId}", userId);

        await _notificationRepository.ClearAllAsync(userId);
        return true;
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}
