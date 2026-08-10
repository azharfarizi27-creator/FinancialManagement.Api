namespace FinancialManagement.Api.DTOs.Dashboard;

public class FinancialSummaryResponse
{
    public decimal TotalBalance { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetBalance { get; set; }
}