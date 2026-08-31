namespace FinancialManagement.Api.Services.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAndSendOtpAsync(string email, string purpose, string? recipientName = null, int expiryMinutes = 10);

    Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose);

    Task<int> CleanupExpiredOtpAsync();
}
