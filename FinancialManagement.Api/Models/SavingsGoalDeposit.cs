namespace FinancialManagement.Api.Models;

public class SavingsGoalDeposit
{
    public int Id { get; set; }

    public int SavingsGoalId { get; set; }

    public int? WalletId { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }

    public SavingsGoal SavingsGoal { get; set; } = null!;

    public Wallet? Wallet { get; set; }
}
