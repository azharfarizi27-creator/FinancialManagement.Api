namespace FinancialManagement.Api.DTOs.User;

public class RestoreBackupResultDto
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = "Data berhasil dipulihkan.";

    public int RestoredWalletsCount { get; set; }

    public int RestoredCategoriesCount { get; set; }

    public int RestoredTransactionsCount { get; set; }

    public int RestoredBudgetsCount { get; set; }

    public int RestoredGoalsCount { get; set; }

    public int RestoredBillsCount { get; set; }
}
