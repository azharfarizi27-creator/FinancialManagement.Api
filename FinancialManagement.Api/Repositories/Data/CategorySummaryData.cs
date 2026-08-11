namespace FinancialManagement.Api.Repositories.Data;

public class CategorySummaryData
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Type { get; set; } = string.Empty;
}