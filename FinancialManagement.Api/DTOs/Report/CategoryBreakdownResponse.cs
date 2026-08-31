namespace FinancialManagement.Api.DTOs.Report;

public class CategoryBreakdownItem
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Type { get; set; } = "Expense";

    public decimal TotalAmount { get; set; }

    public double Percentage { get; set; }

    public int TransactionCount { get; set; }
}

public class CategoryBreakdownResponse
{
    public string Type { get; set; } = "Expense";

    public decimal TotalAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public List<CategoryBreakdownItem> Categories { get; set; } = new();
}
