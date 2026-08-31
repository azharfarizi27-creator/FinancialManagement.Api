namespace FinancialManagement.Api.DTOs.User;

public class UserProfileResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Name => FullName;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public string? PhoneNumber { get; set; }

    public string? Phone => PhoneNumber;

    public string? Bio { get; set; }

    public string? AvatarColor { get; set; }

    public string? AvatarIcon { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Avatar => AvatarUrl;

    public string Theme { get; set; } = "light";

    public string Language { get; set; } = "id";

    public string DateFormat { get; set; } = "DD/MM/YYYY";

    public DateTime CreatedAt { get; set; }

    public DateTime JoinedDate => CreatedAt;
}
