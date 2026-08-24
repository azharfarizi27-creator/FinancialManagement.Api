using FinancialManagement.Api.DTOs.Wallet;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class CreateWalletRequestValidator : AbstractValidator<CreateWalletRequest>
{
    public CreateWalletRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama dompet wajib diisi.")
            .MaximumLength(50).WithMessage("Nama dompet maksimal 50 karakter.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipe dompet wajib diisi.")
            .MaximumLength(50).WithMessage("Tipe dompet maksimal 50 karakter.");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Saldo awal tidak boleh negatif.");
    }
}

public class UpdateWalletRequestValidator : AbstractValidator<UpdateWalletRequest>
{
    public UpdateWalletRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama dompet wajib diisi.")
            .MaximumLength(50).WithMessage("Nama dompet maksimal 50 karakter.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipe dompet wajib diisi.")
            .MaximumLength(50).WithMessage("Tipe dompet maksimal 50 karakter.");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Saldo tidak boleh negatif.");
    }
}
