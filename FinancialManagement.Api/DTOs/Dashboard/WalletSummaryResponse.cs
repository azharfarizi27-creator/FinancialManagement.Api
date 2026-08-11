namespace FinancialManagement.Api.DTOs.Dashboard;

public class WalletSummaryResponse
{
    public int WalletId { get; set; }

    public string WalletName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetBalance { get; set; }
}