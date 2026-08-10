namespace FinancialManagement.Api.Models;

public class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int WalletId { get; set; }

    public int CategoryId { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public Wallet Wallet { get; set; } = null!;

    public Category Category { get; set; } = null!;
}