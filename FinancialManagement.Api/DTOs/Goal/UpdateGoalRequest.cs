namespace FinancialManagement.Api.DTOs.Goal;

public class UpdateGoalRequest
{
    public string Title { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    public string? Category { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public string? Description { get; set; }
}
