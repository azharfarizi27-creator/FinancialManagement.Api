namespace FinancialManagement.Api.DTOs.Notification;

public class CreateNotificationRequest
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "info"; // "info", "warning", "success", "bill", "budget"
}
