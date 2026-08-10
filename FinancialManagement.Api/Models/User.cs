namespace FinancialManagement.Api.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}