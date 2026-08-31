namespace FinancialManagement.Api.DTOs.Auth;

public class OtpCleanupResponse
{
    public bool Success { get; set; } = true;

    public int DeletedCount { get; set; }

    public DateTime CleanedAtUtc { get; set; } = DateTime.UtcNow;

    public string Message { get; set; } = string.Empty;
}
