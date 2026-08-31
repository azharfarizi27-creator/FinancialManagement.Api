namespace FinancialManagement.Api.DTOs.Wallet;

public class TransferResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public decimal TransferredAmount { get; set; }

    public decimal AdminFee { get; set; }

    public decimal FromWalletNewBalance { get; set; }

    public decimal ToWalletNewBalance { get; set; }

    public DateTime TransactionDate { get; set; }
}
