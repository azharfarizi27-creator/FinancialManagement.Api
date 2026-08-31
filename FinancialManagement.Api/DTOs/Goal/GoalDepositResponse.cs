namespace FinancialManagement.Api.DTOs.Goal;

public class GoalDepositResponse
{
    public int Id { get; set; }

    public int SavingsGoalId { get; set; }

    public int? WalletId { get; set; }

    public string? WalletName { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string? Note { get; set; }
}
