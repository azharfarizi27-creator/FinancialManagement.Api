using FinancialManagement.Api.DTOs.Auth;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName wajib diisi.")
            .MaximumLength(100).WithMessage("FullName maksimal 100 karakter.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.")
            .MaximumLength(150).WithMessage("Email maksimal 150 karakter.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi.")
            .MinimumLength(8).WithMessage("Password minimal 8 karakter.")
            .MaximumLength(72).WithMessage("Password maksimal 72 karakter.")
            .Matches(@"[A-Z]").WithMessage("Password harus mengandung minimal 1 huruf besar.")
            .Matches(@"[a-z]").WithMessage("Password harus mengandung minimal 1 huruf kecil.")
            .Matches(@"[0-9]").WithMessage("Password harus mengandung minimal 1 angka.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi.");
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token reset password wajib diisi.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password baru wajib diisi.")
            .MinimumLength(8).WithMessage("Password baru minimal 8 karakter.")
            .MaximumLength(72).WithMessage("Password baru maksimal 72 karakter.")
            .Matches(@"[A-Z]").WithMessage("Password baru harus mengandung minimal 1 huruf besar.")
            .Matches(@"[a-z]").WithMessage("Password baru harus mengandung minimal 1 huruf kecil.")
            .Matches(@"[0-9]").WithMessage("Password baru harus mengandung minimal 1 angka.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Konfirmasi password wajib diisi.")
            .Equal(x => x.NewPassword).WithMessage("Konfirmasi password tidak cocok dengan password baru.");
    }
}
