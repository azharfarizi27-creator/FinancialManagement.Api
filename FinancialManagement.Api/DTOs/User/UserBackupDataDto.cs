namespace FinancialManagement.Api.DTOs.User;

public class UserBackupDataDto
{
    public string AppVersion { get; set; } = "1.0";

    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    public UserProfileBackupDto Profile { get; set; } = new();

    public UserPreferencesDto Preferences { get; set; } = new();

    public List<WalletBackupDto> Wallets { get; set; } = new();

    public List<CategoryBackupDto> Categories { get; set; } = new();

    public List<TransactionBackupDto> Transactions { get; set; } = new();

    public List<BudgetBackupDto> Budgets { get; set; } = new();

    public List<SavingsGoalBackupDto> SavingsGoals { get; set; } = new();

    public List<RecurringBillBackupDto> RecurringBills { get; set; } = new();
}

public class UserProfileBackupDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    public string? AvatarColor { get; set; }

    public string? AvatarIcon { get; set; }

    public string? AvatarUrl { get; set; }
}

public class WalletBackupDto
{
    public string Name { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public string Type { get; set; } = "Cash";
}

public class CategoryBackupDto
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "Expense";
}

public class TransactionBackupDto
{
    public string WalletName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ReceiptUrl { get; set; }

    public DateTime TransactionDate { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class BudgetBackupDto
{
    public string CategoryName { get; set; } = string.Empty;

    public decimal LimitAmount { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }
}

public class SavingsGoalBackupDto
{
    public string Title { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public List<SavingsGoalDepositBackupDto> Deposits { get; set; } = new();
}

public class SavingsGoalDepositBackupDto
{
    public string? WalletName { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string? Note { get; set; }
}

public class RecurringBillBackupDto
{
    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime DueDate { get; set; }

    public string Frequency { get; set; } = "monthly";

    public string? CategoryName { get; set; }

    public int ReminderDays { get; set; } = 3;

    public bool IsPaidThisCycle { get; set; }

    public DateTime? LastPaidDate { get; set; }

    public string? Notes { get; set; }

    public List<RecurringBillPaymentBackupDto> Payments { get; set; } = new();
}

public class RecurringBillPaymentBackupDto
{
    public string? WalletName { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaidDate { get; set; }

    public string? Note { get; set; }
}
