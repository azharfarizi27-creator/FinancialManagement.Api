namespace FinancialManagement.Api.DTOs.User;

public class AccountStatsResponse
{
    public int TotalWallets { get; set; }

    public decimal TotalBalance { get; set; }

    public int TotalCategories { get; set; }

    public int TotalBudgets { get; set; }

    public int TotalGoals { get; set; }

    public int CompletedGoals { get; set; }

    public int TotalBills { get; set; }

    public decimal MonthlyBillCommitment { get; set; }

    public int TotalTransactions { get; set; }
}
