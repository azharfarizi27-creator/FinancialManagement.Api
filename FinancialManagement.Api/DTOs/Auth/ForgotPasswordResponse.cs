namespace FinancialManagement.Api.DTOs.Auth;

public class ForgotPasswordResponse
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = string.Empty;

    public string? ResetToken { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
