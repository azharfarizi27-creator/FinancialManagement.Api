namespace FinancialManagement.Api.DTOs.Goal;

public class GoalResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public double ProgressPercentage => TargetAmount > 0 ? (double)Math.Min(100, Math.Round((CurrentAmount / TargetAmount) * 100, 2)) : 0;

    public DateTime? TargetDate { get; set; }

    public string? Category { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<GoalDepositResponse> History { get; set; } = new();

    public List<GoalDepositResponse> Deposits { get; set; } = new();
}
