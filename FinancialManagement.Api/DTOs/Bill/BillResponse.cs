namespace FinancialManagement.Api.DTOs.Bill;

public class BillResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Name => Title;

    public decimal Amount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount => Math.Max(0, Amount - PaidAmount);

    public DateTime DueDate { get; set; }

    public string Frequency { get; set; } = "monthly";

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public int ReminderDays { get; set; } = 3;

    public bool IsPaidThisCycle { get; set; }

    public DateTime? LastPaidDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<BillPaymentResponse> History { get; set; } = new();

    public List<BillPaymentResponse> Payments { get; set; } = new();
}