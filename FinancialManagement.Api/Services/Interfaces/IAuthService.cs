using FinancialManagement.Api.DTOs.Auth;
using FinancialManagement.Api.DTOs.User;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IAuthService
{
    Task<bool> SendRegisterOtpAsync(SendOtpRequest request);

    Task<AuthResponse> VerifyRegisterOtpAsync(VerifyRegisterOtpRequest request);

    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);

    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);

    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

    Task<bool> SendChangePasswordOtpAsync(int userId);

    Task<bool> ChangePasswordWithOtpAsync(int userId, ChangePasswordRequest request);

    Task<OtpCleanupResponse> CleanupOtpCacheAsync();
}