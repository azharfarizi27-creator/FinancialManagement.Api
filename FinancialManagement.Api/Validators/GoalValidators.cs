using FinancialManagement.Api.DTOs.Goal;
using FluentValidation;

namespace FinancialManagement.Api.Validators;

public class CreateGoalRequestValidator : AbstractValidator<CreateGoalRequest>
{
    public CreateGoalRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Judul target tabungan wajib diisi.")
            .MaximumLength(100).WithMessage("Judul target tabungan maksimal 100 karakter.");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target nominal tabungan harus lebih besar dari 0.");

        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Nominal awal tabungan tidak boleh negatif.");
    }
}

public class UpdateGoalRequestValidator : AbstractValidator<UpdateGoalRequest>
{
    public UpdateGoalRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Judul target tabungan wajib diisi.")
            .MaximumLength(100).WithMessage("Judul target tabungan maksimal 100 karakter.");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target nominal tabungan harus lebih besar dari 0.");
    }
}

public class DepositGoalRequestValidator : AbstractValidator<DepositGoalRequest>
{
    public DepositGoalRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal setoran harus lebih besar dari 0.");
    }
}
