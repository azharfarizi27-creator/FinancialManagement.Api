using FinancialManagement.Api.DTOs.Budget;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
{
    public CreateBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId harus valid (> 0).");

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Batas anggaran (LimitAmount) harus lebih besar dari 0.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Bulan (Month) harus berada di antara 1 dan 12.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Tahun (Year) harus berada di antara 2000 dan 2100.");
    }
}

public class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequest>
{
    public UpdateBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId harus valid (> 0).");

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Batas anggaran (LimitAmount) harus lebih besar dari 0.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Bulan (Month) harus berada di antara 1 dan 12.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Tahun (Year) harus berada di antara 2000 dan 2100.");
    }
}
