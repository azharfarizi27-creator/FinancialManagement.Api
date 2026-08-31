using FinancialManagement.Api.DTOs.Bill;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class CreateBillRequestValidator : AbstractValidator<CreateBillRequest>
{
    public CreateBillRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Nama/judul tagihan wajib diisi.")
            .MaximumLength(100).WithMessage("Nama tagihan maksimal 100 karakter.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal tagihan harus lebih besar dari 0.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frekuensi tagihan wajib diisi.")
            .Must(f => f == "weekly" || f == "monthly" || f == "yearly")
            .WithMessage("Frekuensi harus berupa 'weekly', 'monthly', atau 'yearly'.");

        RuleFor(x => x.ReminderDays)
            .GreaterThanOrEqualTo(0).WithMessage("Hari pengingat tidak boleh negatif.");
    }
}

public class UpdateBillRequestValidator : AbstractValidator<UpdateBillRequest>
{
    public UpdateBillRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Nama/judul tagihan wajib diisi.")
            .MaximumLength(100).WithMessage("Nama tagihan maksimal 100 karakter.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal tagihan harus lebih besar dari 0.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frekuensi tagihan wajib diisi.")
            .Must(f => f == "weekly" || f == "monthly" || f == "yearly")
            .WithMessage("Frekuensi harus berupa 'weekly', 'monthly', atau 'yearly'.");

        RuleFor(x => x.ReminderDays)
            .GreaterThanOrEqualTo(0).WithMessage("Hari pengingat tidak boleh negatif.");
    }
}

public class PayBillRequestValidator : AbstractValidator<PayBillRequest>
{
    public PayBillRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).When(x => x.Amount.HasValue)
            .WithMessage("Nominal pembayaran harus lebih besar dari 0.");
    }
}
