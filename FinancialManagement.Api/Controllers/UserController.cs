using System.Text;
using System.Text.Json;
using FinancialManagement.Api.DTOs.User;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class UserController : BaseApiController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var updated = await _userService.UpdateProfileAsync(userId, request);
        return Ok(updated);
    }

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userId = GetUserId();
        var updated = await _userService.UpdateAvatarAsync(userId, file);
        return Ok(new
        {
            success = true,
            message = "Foto profil berhasil diunggah.",
            avatarUrl = updated.AvatarUrl,
            profile = updated
        });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = GetUserId();
        await _userService.ChangePasswordAsync(userId, request);
        return Ok(new
        {
            success = true,
            message = "Kata sandi berhasil diperbarui."
        });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetUserId();
        var stats = await _userService.GetAccountStatsAsync(userId);
        return Ok(stats);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = GetUserId();
        var preferences = await _userService.GetPreferencesAsync(userId);
        return Ok(preferences);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(UserPreferencesDto request)
    {
        var userId = GetUserId();
        var updated = await _userService.UpdatePreferencesAsync(userId, request);
        return Ok(updated);
    }

    [HttpGet("backup")]
    public async Task<IActionResult> ExportBackup([FromQuery] bool asFile = true)
    {
        var userId = GetUserId();
        var backupData = await _userService.ExportBackupDataAsync(userId);

        if (asFile)
        {
            var json = JsonSerializer.Serialize(backupData, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            var bytes = Encoding.UTF8.GetBytes(json);
            var fileName = $"financial_backup_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            return File(bytes, "application/json", fileName);
        }

        return Ok(backupData);
    }

    [HttpPost("restore")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RestoreBackup(
        IFormFile file,
        [FromQuery] bool overwrite = false)
    {
        var userId = GetUserId();

        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "File backup JSON wajib diunggah."
            });
        }

        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
        var content = await reader.ReadToEndAsync();
        var backupData = JsonSerializer.Deserialize<UserBackupDataDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (backupData == null)
        {
            return BadRequest(new
            {
                message = "File backup JSON tidak valid atau rusak."
            });
        }

        var result = await _userService.RestoreBackupDataAsync(userId, backupData, overwrite);
        return Ok(result);
    }

    [HttpPost("restore-json")]
    public async Task<IActionResult> RestoreBackupJson(
        [FromBody] UserBackupDataDto backupData,
        [FromQuery] bool overwrite = false)
    {
        var userId = GetUserId();

        if (backupData == null)
        {
            return BadRequest(new
            {
                message = "Payload backup JSON tidak valid."
            });
        }

        var result = await _userService.RestoreBackupDataAsync(userId, backupData, overwrite);
        return Ok(result);
    }
}
