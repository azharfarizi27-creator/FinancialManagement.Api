using System.Security.Cryptography;
using FinancialManagement.Api.Data;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Services.Impl;

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;

    public OtpService(
        AppDbContext context,
        IEmailService emailService,
        ILogger<OtpService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> GenerateAndSendOtpAsync(string email, string purpose, string? recipientName = null, int expiryMinutes = 10)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        _logger.LogInformation("Membuat OTP untuk {Email} dengan tujuan {Purpose}", normalizedEmail, purpose);

        // Nonaktifkan OTP sebelumnya yang belum digunakan untuk email dan tujuan yang sama
        var oldOtps = await _context.OtpTokens
            .Where(o => o.Email == normalizedEmail && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync();

        foreach (var old in oldOtps)
        {
            old.IsUsed = true;
        }

        // Generate 6-digit numeric OTP cryptographically
        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var otpEntity = new OtpToken
        {
            Email = normalizedEmail,
            Code = otpCode,
            Purpose = purpose,
            ExpiresAt = expiresAt,
            IsUsed = false,
            Attempts = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.OtpTokens.Add(otpEntity);
        await _context.SaveChangesAsync();

        // Kirim email asli berisi kode OTP
        await _emailService.SendOtpEmailAsync(normalizedEmail, otpCode, purpose, recipientName);

        _logger.LogInformation("Kode OTP berhasil dibuat dan dikirim ke {Email} untuk tujuan {Purpose}", normalizedEmail, purpose);

        return otpCode;
    }

    public async Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedCode = otpCode?.Trim() ?? string.Empty;

        _logger.LogInformation("Memvalidasi OTP untuk {Email} dengan tujuan {Purpose}", normalizedEmail, purpose);

        var validOtp = await _context.OtpTokens
            .Where(o => o.Email == normalizedEmail && o.Purpose == purpose && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (validOtp == null)
        {
            _logger.LogWarning("Validasi OTP gagal: Tidak ada OTP aktif untuk {Email} dengan tujuan {Purpose}", normalizedEmail, purpose);
            throw new BadRequestException("Kode OTP tidak ditemukan atau sudah tidak berlaku. Silakan minta kode OTP baru.");
        }

        if (validOtp.ExpiresAt <= DateTime.UtcNow)
        {
            validOtp.IsUsed = true;
            await _context.SaveChangesAsync();
            _logger.LogWarning("Validasi OTP gagal: Kode OTP sudah kedaluwarsa untuk {Email}", normalizedEmail);
            throw new BadRequestException("Kode OTP telah kedaluwarsa. Silakan minta kode OTP baru.");
        }

        if (validOtp.Attempts >= 5)
        {
            validOtp.IsUsed = true;
            await _context.SaveChangesAsync();
            _logger.LogWarning("Validasi OTP gagal: Batas percobaan tercapai untuk {Email}", normalizedEmail);
            throw new BadRequestException("Batas maksimal percobaan OTP telah tercapai. Silakan minta kode OTP baru.");
        }

        if (validOtp.Code != normalizedCode)
        {
            validOtp.Attempts++;
            await _context.SaveChangesAsync();
            _logger.LogWarning("Validasi OTP gagal: Kode salah untuk {Email}. Percobaan ke-{Attempts}", normalizedEmail, validOtp.Attempts);
            throw new BadRequestException($"Kode OTP salah. Sisa percobaan: {5 - validOtp.Attempts}.");
        }

        // Tandai sebagai terpakai
        validOtp.IsUsed = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP berhasil divalidasi untuk {Email} ({Purpose})", normalizedEmail, purpose);

        return true;
    }

    public async Task<int> CleanupExpiredOtpAsync()
    {
        _logger.LogInformation("Menjalankan proses pembersihan (cleanup) data sampah OTP di database...");

        var now = DateTime.UtcNow;
        var staleOtps = await _context.OtpTokens
            .Where(o => o.IsUsed || o.ExpiresAt < now)
            .ToListAsync();

        var count = staleOtps.Count;

        if (count > 0)
        {
            _context.OtpTokens.RemoveRange(staleOtps);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Pembersihan selesai: {Count} record OTP sampah berhasil dihapus dari database.", count);
        }
        else
        {
            _logger.LogInformation("Pembersihan selesai: Tidak ada record OTP sampah yang perlu dihapus.");
        }

        return count;
    }
}
