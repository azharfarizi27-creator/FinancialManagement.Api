namespace FinancialManagement.Api.Models;

public class SavingsGoal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    public string? Category { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public ICollection<SavingsGoalDeposit> Deposits { get; set; } = new List<SavingsGoalDeposit>();
}
