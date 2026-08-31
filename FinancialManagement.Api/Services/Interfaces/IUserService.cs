using FinancialManagement.Api.DTOs.User;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(int userId);

    Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    Task<UserProfileResponse> UpdateAvatarAsync(int userId, IFormFile file);

    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);

    Task<AccountStatsResponse> GetAccountStatsAsync(int userId);

    Task<UserPreferencesDto> GetPreferencesAsync(int userId);

    Task<UserPreferencesDto> UpdatePreferencesAsync(int userId, UserPreferencesDto request);

    Task<UserBackupDataDto> ExportBackupDataAsync(int userId);

    Task<RestoreBackupResultDto> RestoreBackupDataAsync(int userId, UserBackupDataDto backupData, bool overwrite = false);
}
