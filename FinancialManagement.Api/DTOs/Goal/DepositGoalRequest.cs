namespace FinancialManagement.Api.DTOs.Goal;

public class DepositGoalRequest
{
    public decimal Amount { get; set; }

    public int? WalletId { get; set; }

    public string? Note { get; set; }

    public DateTime? Date { get; set; }
}
