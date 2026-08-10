namespace FinancialManagement.Api.DTOs.Dashboard;

public class RecentTransactionResponse
{
    public int Id { get; set; }

    public string WalletName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; }
}