using System.Net;
using System.Net.Mail;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpHost = _configuration["SmtpSettings:Host"];
        var smtpPort = _configuration.GetValue<int>("SmtpSettings:Port", 587);
        var enableSsl = _configuration.GetValue<bool>("SmtpSettings:EnableSsl", true);
        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "noreply@dompetin.com";
        var senderName = _configuration["SmtpSettings:SenderName"] ?? "Dompetin Financial";
        var username = _configuration["SmtpSettings:Username"];
        var password = _configuration["SmtpSettings:Password"];

        _logger.LogInformation("Mengirim email ke {ToEmail} dengan subjek: {Subject}", toEmail, subject);

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogWarning("SMTP Host belum dikonfigurasi di appsettings.json. Email tidak dapat dikirim ke {ToEmail}.", toEmail);
            throw new BadRequestException("Layanan email belum dikonfigurasi oleh administrator server.");
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email berhasil terkirim ke {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gagal mengirim email ke {ToEmail} melalui SMTP {SmtpHost}:{SmtpPort}", toEmail, smtpHost, smtpPort);
            throw new BadRequestException($"Gagal mengirim email verifikasi ke {toEmail}. Pastikan konfigurasi SMTP benar: {ex.Message}");
        }
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose, string? recipientName = null)
    {
        var greeting = !string.IsNullOrWhiteSpace(recipientName) ? $"Halo {recipientName}," : "Halo,";
        
        string purposeTitle;
        string purposeDescription;
        string actionInfo;

        switch (purpose?.ToLowerInvariant())
        {
            case "register":
                purposeTitle = "Verifikasi Pendaftaran Akun";
                purposeDescription = "Terima kasih telah mendaftar di <strong>Dompetin Financial Management</strong>. Gunakan kode OTP berikut untuk menyelesaikan pendaftaran akun Anda:";
                actionInfo = "Kode ini hanya berlaku selama <strong>10 menit</strong>.";
                break;
            case "resetpassword":
                purposeTitle = "Permintaan Reset Kata Sandi";
                purposeDescription = "Kami menerima permintaan untuk menyetel ulang kata sandi akun Dompetin Anda. Gunakan kode OTP berikut untuk melanjutkan proses reset kata sandi:";
                actionInfo = "Kode ini hanya berlaku selama <strong>15 menit</strong>. Jika Anda tidak meminta reset kata sandi, abaikan email ini.";
                break;
            case "changepassword":
                purposeTitle = "Verifikasi Perubahan Kata Sandi";
                purposeDescription = "Kami menerima permintaan untuk mengganti kata sandi akun Anda. Gunakan kode OTP berikut untuk memverifikasi identitas Anda:";
                actionInfo = "Kode ini hanya berlaku selama <strong>10 menit</strong>.";
                break;
            default:
                purposeTitle = "Kode Verifikasi Keamanan (OTP)";
                purposeDescription = "Berikut adalah kode verifikasi OTP Anda untuk mengakses layanan Dompetin:";
                actionInfo = "Kode ini hanya berlaku selama <strong>10 menit</strong>.";
                break;
        }

        var subject = $"[{otpCode}] {purposeTitle} - Dompetin";

        var htmlBody = $@"
<!DOCTYPE html>
<html lang=""id"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{purposeTitle}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background-color: #f1f5f9;
            margin: 0;
            padding: 24px;
            color: #1e293b;
        }}
        .container {{
            max-width: 540px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 16px;
            box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
            overflow: hidden;
            border: 1px solid #e2e8f0;
        }}
        .header {{
            background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
            padding: 32px 24px;
            text-align: center;
            color: #ffffff;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 700;
            letter-spacing: -0.5px;
        }}
        .header p {{
            margin: 6px 0 0 0;
            font-size: 14px;
            opacity: 0.9;
        }}
        .content {{
            padding: 32px 28px;
        }}
        .greeting {{
            font-size: 16px;
            font-weight: 600;
            margin-bottom: 12px;
            color: #0f172a;
        }}
        .description {{
            font-size: 14px;
            line-height: 1.6;
            color: #475569;
            margin-bottom: 24px;
        }}
        .otp-box {{
            background: #f8fafc;
            border: 2px dashed #93c5fd;
            border-radius: 12px;
            padding: 20px;
            text-align: center;
            margin: 24px 0;
        }}
        .otp-code {{
            font-family: 'Courier New', Courier, monospace;
            font-size: 36px;
            font-weight: 800;
            letter-spacing: 8px;
            color: #1d4ed8;
            margin: 0;
            display: inline-block;
        }}
        .expiry-note {{
            font-size: 13px;
            color: #64748b;
            margin-top: 8px;
        }}
        .security-warning {{
            background-color: #fef2f2;
            border-left: 4px solid #ef4444;
            padding: 12px 16px;
            border-radius: 6px;
            margin-top: 24px;
            font-size: 12px;
            line-height: 1.5;
            color: #991b1b;
        }}
        .footer {{
            background-color: #f8fafc;
            padding: 20px 24px;
            text-align: center;
            border-top: 1px solid #e2e8f0;
            font-size: 12px;
            color: #94a3b8;
        }}
        .footer a {{
            color: #2563eb;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Dompetin Financial</h1>
            <p>{purposeTitle}</p>
        </div>
        <div class=""content"">
            <div class=""greeting"">{greeting}</div>
            <div class=""description"">
                {purposeDescription}
            </div>
            
            <div class=""otp-box"">
                <div class=""otp-code"">{otpCode}</div>
                <div class=""expiry-note"">{actionInfo}</div>
            </div>

            <div class=""security-warning"">
                <strong>Penting:</strong> Jangan pernah membagikan kode OTP ini kepada siapa pun, termasuk pihak yang mengatasnamakan Dompetin. Tim kami tidak akan pernah meminta kode rahasia Anda.
            </div>
        </div>
        <div class=""footer"">
            &copy; {DateTime.UtcNow.Year} Dompetin Financial Management. Hak cipta dilindungi undang-undang.<br>
            Email otomatis, mohon tidak membalas langsung ke alamat email ini.
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }
}
