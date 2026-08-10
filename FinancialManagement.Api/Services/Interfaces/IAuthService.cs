using FinancialManagement.Api.DTOs.Auth;
using FinancialManagement.Api.Services.Impl;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);
}