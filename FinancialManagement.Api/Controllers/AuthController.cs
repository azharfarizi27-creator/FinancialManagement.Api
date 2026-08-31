using System.Security.Claims;
using FinancialManagement.Api.DTOs.Auth;
using FinancialManagement.Api.DTOs.User;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(
        IAuthService authService,
        IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    /// <summary>
    /// Mengirim kode OTP verifikasi pendaftaran ke email calon pengguna.
    /// </summary>
    [HttpPost("register/send-otp")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> SendRegisterOtp([FromBody] SendOtpRequest request)
    {
        await _authService.SendRegisterOtpAsync(request);
        return Ok(new
        {
            success = true,
            message = "Kode OTP verifikasi berhasil dikirim ke email Anda. Silakan cek kotak masuk atau folder spam."
        });
    }

    /// <summary>
    /// Memverifikasi kode OTP dan menyelesaikan proses pendaftaran akun baru.
    /// </summary>
    [HttpPost("register/verify-otp")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyRegisterOtpRequest request)
    {
        var result = await _authService.VerifyRegisterOtpAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Pendaftaran akun langsung (Direct Registration).
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Masuk (Login) ke dalam sistem dengan email dan kata sandi.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Email atau password salah."
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Mengirim kode OTP reset kata sandi ke email pengguna.
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Menyetel ulang kata sandi dengan kode OTP yang dikirim ke email.
    /// </summary>
    [HttpPost("reset-password")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(new
        {
            success = true,
            message = "Kata sandi berhasil disetel ulang. Silakan login dengan kata sandi baru Anda."
        });
    }

    /// <summary>
    /// Mengirim kode OTP ke email pengguna yang sedang login untuk konfirmasi pergantian kata sandi.
    /// </summary>
    [Authorize]
    [HttpPost("change-password/send-otp")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> SendChangePasswordOtp()
    {
        var userId = GetUserId();
        await _authService.SendChangePasswordOtpAsync(userId);
        return Ok(new
        {
            success = true,
            message = "Kode OTP ganti kata sandi berhasil dikirim ke alamat email Anda."
        });
    }

    /// <summary>
    /// Mengubah kata sandi pengguna yang sedang login (dengan verifikasi OTP jika disertakan).
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        await _authService.ChangePasswordWithOtpAsync(userId, request);
        return Ok(new
        {
            success = true,
            message = "Kata sandi berhasil diperbarui."
        });
    }

    /// <summary>
    /// Membersihkan (cleanup) kode OTP yang kedaluwarsa atau sudah terpakai dari database agar database tetap ringan.
    /// </summary>
    [HttpDelete("otp/cleanup")]
    public async Task<IActionResult> CleanupExpiredOtpDelete()
    {
        var result = await _authService.CleanupOtpCacheAsync();
        return Ok(result);
    }

    /// <summary>
    /// Endpoint alternatif (POST) untuk membersihkan record sampah kode OTP di database.
    /// </summary>
    [HttpPost("otp/cleanup")]
    public async Task<IActionResult> CleanupExpiredOtpPost()
    {
        var result = await _authService.CleanupOtpCacheAsync();
        return Ok(result);
    }

    /// <summary>
    /// Mendapatkan data identitas pengguna yang sedang login dari JWT token.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = GetUserId();
        var fullName = User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            userId,
            fullName,
            email,
            role
        });
    }
}