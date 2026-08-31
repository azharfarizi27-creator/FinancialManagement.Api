namespace FinancialManagement.Api.DTOs.Bill;

public class CreateBillRequest
{
    public string Title { get; set; } = string.Empty;

    // Optional alias for Name in frontend payloads
    public string? Name { get; set; }

    public decimal Amount { get; set; }

    public DateTime DueDate { get; set; }

    public string Frequency { get; set; } = "monthly"; // "weekly", "monthly", "yearly"

    public int? CategoryId { get; set; }

    public int ReminderDays { get; set; } = 3;

    public string? Notes { get; set; }
}
