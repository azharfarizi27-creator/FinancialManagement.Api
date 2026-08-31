namespace FinancialManagement.Api.DTOs.Bill;

public class PayBillRequest
{
    public int? WalletId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Note { get; set; }
}
