namespace FinancialManagement.Api.DTOs.Goal;

public class CreateGoalRequest
{
    public string Title { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; } = 0;

    public DateTime? TargetDate { get; set; }

    public string? Category { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public string? Description { get; set; }

    public int? InitialDepositWalletId { get; set; }
}
