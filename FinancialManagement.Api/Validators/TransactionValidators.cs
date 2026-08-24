using FinancialManagement.Api.DTOs.Transaction;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.WalletId)
            .GreaterThan(0).WithMessage("WalletId harus valid (> 0).");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId harus valid (> 0).");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal transaksi (Amount) harus lebih besar dari 0.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipe transaksi wajib diisi.")
            .Must(t => t == "Income" || t == "Expense")
            .WithMessage("Tipe transaksi harus 'Income' atau 'Expense'.");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Deskripsi transaksi maksimal 255 karakter.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Tanggal transaksi wajib diisi.");
    }
}

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.WalletId)
            .GreaterThan(0).WithMessage("WalletId harus valid (> 0).");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId harus valid (> 0).");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal transaksi (Amount) harus lebih besar dari 0.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipe transaksi wajib diisi.")
            .Must(t => t == "Income" || t == "Expense")
            .WithMessage("Tipe transaksi harus 'Income' atau 'Expense'.");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Deskripsi transaksi maksimal 255 karakter.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Tanggal transaksi wajib diisi.");
    }
}
