namespace FinancialManagement.Api.DTOs.User;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string? OtpCode { get; set; }
}
