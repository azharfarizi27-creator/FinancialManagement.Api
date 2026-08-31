namespace FinancialManagement.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);

    Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose, string? recipientName = null);
}
