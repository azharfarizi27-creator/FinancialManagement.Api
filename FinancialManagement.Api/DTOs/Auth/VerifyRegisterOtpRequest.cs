namespace FinancialManagement.Api.DTOs.Auth;

public class VerifyRegisterOtpRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string OtpCode { get; set; } = string.Empty;
}
