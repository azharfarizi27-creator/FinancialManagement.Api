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

public class TransferWalletRequestValidator : AbstractValidator<TransferWalletRequest>
{
    public TransferWalletRequestValidator()
    {
        RuleFor(x => x.FromWalletId)
            .GreaterThan(0).WithMessage("Dompet asal wajib dipilih.");

        RuleFor(x => x.ToWalletId)
            .GreaterThan(0).WithMessage("Dompet tujuan wajib dipilih.")
            .NotEqual(x => x.FromWalletId).WithMessage("Dompet asal dan tujuan tidak boleh sama.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal transfer harus lebih besar dari 0.");

        RuleFor(x => x.AdminFee)
            .GreaterThanOrEqualTo(0).WithMessage("Biaya admin tidak boleh negatif.");
    }
}

