namespace FinancialManagement.Api.Models;

public class RecurringBill
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime DueDate { get; set; }

    public string Frequency { get; set; } = "monthly"; // "weekly", "monthly", "yearly"

    public int? CategoryId { get; set; }

    public int ReminderDays { get; set; } = 3;

    public bool IsPaidThisCycle { get; set; }

    public DateTime? LastPaidDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public Category? Category { get; set; }

    public ICollection<RecurringBillPayment> Payments { get; set; } = new List<RecurringBillPayment>();
}
