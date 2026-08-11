namespace FinancialManagement.Api.DTOs.Budget;

public class BudgetResponse
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal LimitAmount { get; set; }

    public decimal UsedAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateTime CreatedAt { get; set; }
}