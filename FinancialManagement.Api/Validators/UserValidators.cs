using FinancialManagement.Api.DTOs.User;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nama lengkap wajib diisi.")
            .MaximumLength(100).WithMessage("Nama lengkap maksimal 100 karakter.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Nomor HP maksimal 20 karakter.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio maksimal 500 karakter.")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Password saat ini wajib diisi.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password baru wajib diisi.")
            .MinimumLength(6).WithMessage("Password baru minimal 6 karakter.");
    }
}

public class UserPreferencesDtoValidator : AbstractValidator<UserPreferencesDto>
{
    private static readonly string[] AllowedCurrencies = { "IDR", "USD", "EUR", "SGD" };
    private static readonly string[] AllowedNumberFormats = { "full", "compact" };
    private static readonly string[] AllowedThemes = { "light", "dark", "system" };
    private static readonly string[] AllowedLanguages = { "id", "en" };
    private static readonly string[] AllowedDateFormats = { "DD/MM/YYYY", "MM/DD/YYYY", "YYYY-MM-DD", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };

    public UserPreferencesDtoValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency wajib diisi.")
            .Must(c => AllowedCurrencies.Contains(c.ToUpper()))
            .WithMessage($"Currency tidak valid. Pilihan: {string.Join(", ", AllowedCurrencies)}");

        RuleFor(x => x.NumberFormat)
            .NotEmpty().WithMessage("NumberFormat wajib diisi.")
            .Must(f => AllowedNumberFormats.Contains(f.ToLower()))
            .WithMessage($"NumberFormat tidak valid. Pilihan: {string.Join(", ", AllowedNumberFormats)}");

        RuleFor(x => x.Theme)
            .NotEmpty().WithMessage("Theme wajib diisi.")
            .Must(t => AllowedThemes.Contains(t.ToLower()))
            .WithMessage($"Theme tidak valid. Pilihan: {string.Join(", ", AllowedThemes)}");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language wajib diisi.")
            .Must(l => AllowedLanguages.Contains(l.ToLower()))
            .WithMessage($"Language tidak valid. Pilihan: {string.Join(", ", AllowedLanguages)}");

        RuleFor(x => x.DateFormat)
            .NotEmpty().WithMessage("DateFormat wajib diisi.")
            .Must(d => AllowedDateFormats.Contains(d))
            .WithMessage($"DateFormat tidak valid. Pilihan: DD/MM/YYYY, MM/DD/YYYY, YYYY-MM-DD");
    }
}
