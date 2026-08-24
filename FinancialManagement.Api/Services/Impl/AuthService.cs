using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.Auth;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinancialManagement.Api.Services.Impl;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        _logger.LogInformation("Mencoba mendaftarkan user baru dengan email: {Email}", request.Email);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
        {
            _logger.LogWarning("Pendaftaran gagal: Email {Email} sudah terdaftar", request.Email);
            throw new ConflictException("Email sudah terdaftar dalam sistem.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
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

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower());

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