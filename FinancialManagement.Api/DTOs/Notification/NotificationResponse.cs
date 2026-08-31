namespace FinancialManagement.Api.DTOs.Notification;

public class NotificationResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "info";

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UnreadNotificationCountResponse
{
    public int UnreadCount { get; set; }
}
