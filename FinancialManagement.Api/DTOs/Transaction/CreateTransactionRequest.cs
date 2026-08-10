namespace FinancialManagement.Api.DTOs.Transaction;

public class CreateTransactionRequest
{
    public int WalletId { get; set; }

    public int CategoryId { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; }
}