namespace FinancialManagement.Api.DTOs.User;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Phone { get; set; }

    public string? Bio { get; set; }

    public string? AvatarColor { get; set; }

    public string? AvatarIcon { get; set; }
}
