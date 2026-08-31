using FinancialManagement.Api.DTOs.Notification;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class NotificationController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, limit);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification(CreateNotificationRequest request)
    {
        var userId = GetUserId();
        var notification = await _notificationService.CreateNotificationAsync(userId, request);
        return Ok(notification);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        await _notificationService.MarkAsReadAsync(id, userId);
        return Ok(new
        {
            success = true,
            message = "Notifikasi berhasil ditandai sebagai telah dibaca."
        });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new
        {
            success = true,
            message = "Semua notifikasi berhasil ditandai sebagai telah dibaca."
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var userId = GetUserId();
        await _notificationService.DeleteNotificationAsync(id, userId);
        return Ok(new
        {
            success = true,
            message = "Notifikasi berhasil dihapus."
        });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearAllNotifications()
    {
        var userId = GetUserId();
        await _notificationService.ClearAllNotificationsAsync(userId);
        return Ok(new
        {
            success = true,
            message = "Semua notifikasi berhasil dibersihkan."
        });
    }
}
