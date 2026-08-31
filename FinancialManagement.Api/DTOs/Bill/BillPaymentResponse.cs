namespace FinancialManagement.Api.DTOs.Bill;

public class BillPaymentResponse
{
    public int Id { get; set; }

    public int RecurringBillId { get; set; }

    public int? WalletId { get; set; }

    public string? WalletName { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string? Note { get; set; }
}
