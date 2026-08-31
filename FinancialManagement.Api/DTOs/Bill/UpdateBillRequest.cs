namespace FinancialManagement.Api.DTOs.Bill;

public class UpdateBillRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Name { get; set; }

    public decimal Amount { get; set; }

    public DateTime DueDate { get; set; }

    public string Frequency { get; set; } = "monthly";

    public int? CategoryId { get; set; }

    public int ReminderDays { get; set; } = 3;

    public bool? IsPaidThisCycle { get; set; }

    public string? Notes { get; set; }
}
