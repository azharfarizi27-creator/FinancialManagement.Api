namespace FinancialManagement.Api.DTOs.Wallet;

public class TransferWalletRequest
{
    public int FromWalletId { get; set; }

    public int ToWalletId { get; set; }

    public decimal Amount { get; set; }

    public decimal AdminFee { get; set; } = 0;

    public DateTime? Date { get; set; }

    public string? Notes { get; set; }
}
