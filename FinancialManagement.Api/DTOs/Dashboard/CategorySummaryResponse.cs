namespace FinancialManagement.Api.DTOs.Dashboard;

public class CategorySummaryResponse
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Type { get; set; } = string.Empty;
}