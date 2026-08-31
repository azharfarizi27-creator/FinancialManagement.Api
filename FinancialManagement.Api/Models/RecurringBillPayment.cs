namespace FinancialManagement.Api.Models;

public class RecurringBillPayment
{
    public int Id { get; set; }

    public int RecurringBillId { get; set; }

    public int? WalletId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaidDate { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }

    public RecurringBill RecurringBill { get; set; } = null!;

    public Wallet? Wallet { get; set; }
}
