using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.Auth;
using FinancialManagement.Api.DTOs.User;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinancialManagement.Api.Services.Impl;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IOtpService otpService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _otpService = otpService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendRegisterOtpAsync(SendOtpRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        _logger.LogInformation("Memproses permintaan OTP registrasi untuk email: {Email}", normalizedEmail);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (existingUser != null)
        {
            _logger.LogWarning("Gagal kirim OTP registrasi: Email {Email} sudah terdaftar", normalizedEmail);
            throw new ConflictException("Email sudah terdaftar dalam sistem. Silakan gunakan email lain atau login.");
        }

        await _otpService.GenerateAndSendOtpAsync(normalizedEmail, "Register", expiryMinutes: 10);
        return true;
    }

    public async Task<AuthResponse> VerifyRegisterOtpAsync(VerifyRegisterOtpRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        _logger.LogInformation("Memverifikasi OTP dan mendaftarkan akun untuk email: {Email}", normalizedEmail);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (existingUser != null)
        {
            _logger.LogWarning("Pendaftaran gagal: Email {Email} sudah terdaftar", normalizedEmail);
            throw new ConflictException("Email sudah terdaftar dalam sistem.");
        }

        // Verifikasi kode OTP
        await _otpService.ValidateOtpAsync(normalizedEmail, request.OtpCode, "Register");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User baru berhasil didaftarkan via verifikasi OTP: UserId {UserId}, Email {Email}", user.Id, user.Email);

        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = GenerateToken(user)
        };
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        _logger.LogInformation("Mencoba mendaftarkan user baru dengan email: {Email}", request.Email);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (existingUser != null)
        {
            _logger.LogWarning("Pendaftaran gagal: Email {Email} sudah terdaftar", request.Email);
            throw new ConflictException("Email sudah terdaftar dalam sistem.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password
            ),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User baru berhasil didaftarkan: UserId {UserId}, Email {Email}", user.Id, user.Email);

        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = GenerateToken(user)
        };
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request)
    {
        _logger.LogInformation("Mencoba login untuk email: {Email}", request.Email);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            _logger.LogWarning("Login gagal: User dengan email {Email} tidak ditemukan", request.Email);
            return null;
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!passwordValid)
        {
            _logger.LogWarning("Login gagal: Password tidak valid untuk email {Email} (UserId: {UserId})", request.Email, user.Id);
            return null;
        }

        _logger.LogInformation("Login berhasil untuk UserId {UserId}, Email {Email}", user.Id, user.Email);

        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = GenerateToken(user)
        };
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        _logger.LogInformation("Permintaan lupa kata sandi untuk email: {Email}", request.Email);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            _logger.LogWarning("User dengan email {Email} tidak ditemukan untuk forgot-password", request.Email);
            // Kembalikan response sukses umum untuk keamanan
            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "Jika email Anda terdaftar, kode OTP reset kata sandi telah dikirim ke email Anda."
            };
        }

        // Generate dan kirim kode OTP reset password ke email user
        var otpCode = await _otpService.GenerateAndSendOtpAsync(user.Email, "ResetPassword", user.FullName, expiryMinutes: 15);

        _logger.LogInformation("OTP reset password dikirim ke email untuk UserId {UserId}", user.Id);

        return new ForgotPasswordResponse
        {
            Success = true,
            Message = "Kode OTP reset kata sandi telah dikirim ke email Anda dan berlaku selama 15 menit.",
            ResetToken = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        _logger.LogInformation("Mencoba menyetel ulang kata sandi untuk email: {Email}", request.Email);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            throw new NotFoundException("Akun dengan email tersebut tidak ditemukan.");
        }

        var tokenOrOtp = !string.IsNullOrWhiteSpace(request.OtpCode) ? request.OtpCode.Trim() : request.Token.Trim();
        if (string.IsNullOrWhiteSpace(tokenOrOtp))
        {
            throw new BadRequestException("Kode OTP atau Token reset kata sandi wajib diisi.");
        }

        // 1. Coba validasi melalui OtpService
        bool isOtpValid = false;
        try
        {
            isOtpValid = await _otpService.ValidateOtpAsync(normalizedEmail, tokenOrOtp, "ResetPassword");
        }
        catch (BadRequestException)
        {
            // 2. Fallback: Coba validasi legacy PasswordResetTokens jika belum lewat OtpToken
            var legacyToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id &&
                                          t.Token == tokenOrOtp &&
                                          !t.IsUsed &&
                                          t.ExpiresAt > DateTime.UtcNow);

            if (legacyToken != null)
            {
                legacyToken.IsUsed = true;
                isOtpValid = true;
            }
            else
            {
                throw;
            }
        }

        if (!isOtpValid)
        {
            throw new BadRequestException("Kode OTP reset kata sandi tidak valid atau sudah kedaluwarsa.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Kata sandi berhasil disetel ulang untuk UserId {UserId}", user.Id);

        return true;
    }

    public async Task<bool> SendChangePasswordOtpAsync(int userId)
    {
        _logger.LogInformation("Mengirim OTP ganti password untuk UserId {UserId}", userId);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        await _otpService.GenerateAndSendOtpAsync(user.Email, "ChangePassword", user.FullName, expiryMinutes: 10);
        return true;
    }

    public async Task<bool> ChangePasswordWithOtpAsync(int userId, ChangePasswordRequest request)
    {
        _logger.LogInformation("Mencoba mengubah password dengan verifikasi OTP untuk UserId {UserId}", userId);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword,
            user.PasswordHash);

        if (!isOldPasswordValid)
        {
            _logger.LogWarning("Gagal ganti password: Password saat ini salah untuk UserId {UserId}", userId);
            throw new BadRequestException("Kata sandi saat ini tidak sesuai.");
        }

        // Jika OtpCode disertakan, validasi OTP
        if (!string.IsNullOrWhiteSpace(request.OtpCode))
        {
            await _otpService.ValidateOtpAsync(user.Email, request.OtpCode.Trim(), "ChangePassword");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Kata sandi berhasil diperbarui untuk UserId {UserId}", userId);
        return true;
    }

    public async Task<OtpCleanupResponse> CleanupOtpCacheAsync()
    {
        _logger.LogInformation("Memulai pembersihan cache dan record sampah OTP...");
        var deletedCount = await _otpService.CleanupExpiredOtpAsync();

        return new OtpCleanupResponse
        {
            Success = true,
            DeletedCount = deletedCount,
            CleanedAtUtc = DateTime.UtcNow,
            Message = $"Berhasil membersihkan {deletedCount} record kode OTP yang kedaluwarsa/tidak terpakai."
        };
    }

    private string GenerateToken(User user)
    {
        _logger.LogDebug("Membuat JWT token untuk UserId {UserId}, Role {Role}", user.Id, user.Role);

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),
            new Claim(
                ClaimTypes.Name,
                user.FullName
            ),
            new Claim(
                ClaimTypes.Email,
                user.Email
            ),
            new Claim(
                ClaimTypes.Role,
                user.Role
            )
        };

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Konfigurasi Jwt:Key tidak ditemukan.");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var expirationMinutes =
            _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                expirationMinutes
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}