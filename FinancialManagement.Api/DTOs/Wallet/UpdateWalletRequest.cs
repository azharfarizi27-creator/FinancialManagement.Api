namespace FinancialManagement.Api.DTOs.Wallet;

public class UpdateWalletRequest
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public decimal Balance { get; set; }
}