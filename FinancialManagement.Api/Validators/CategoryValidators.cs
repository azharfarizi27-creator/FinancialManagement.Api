using FinancialManagement.Api.DTOs.Category;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama kategori wajib diisi.")
            .MaximumLength(50).WithMessage("Nama kategori maksimal 50 karakter.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipe kategori wajib diisi.")
            .Must(t => t == "Income" || t == "Expense")
            .WithMessage("Tipe kategori harus 'Income' atau 'Expense'.");
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama kategori wajib diisi.")
            .MaximumLength(50).WithMessage("Nama kategori maksimal 50 karakter.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipe kategori wajib diisi.")
            .Must(t => t == "Income" || t == "Expense")
            .WithMessage("Tipe kategori harus 'Income' atau 'Expense'.");
    }
}
