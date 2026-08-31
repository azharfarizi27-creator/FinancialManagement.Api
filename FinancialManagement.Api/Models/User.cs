namespace FinancialManagement.Api.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    public string? AvatarColor { get; set; }

    public string? AvatarIcon { get; set; }

    public string? AvatarUrl { get; set; }

    // User Preferences
    public string Currency { get; set; } = "IDR";

    public string NumberFormat { get; set; } = "full";

    public string Theme { get; set; } = "light";

    public string Language { get; set; } = "id";

    public string DateFormat { get; set; } = "DD/MM/YYYY";

    public bool BillReminder { get; set; } = true;

    public bool BudgetAlert { get; set; } = true;

    public bool WeeklyDigest { get; set; } = false;

    public bool NotifyGoalMilestone { get; set; } = true;

    public bool NotifyBillDue { get; set; } = true;

    public bool NotifyBudgetLimit { get; set; } = true;

    public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();

    public ICollection<RecurringBill> RecurringBills { get; set; } = new List<RecurringBill>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}