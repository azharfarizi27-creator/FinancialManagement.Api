namespace FinancialManagement.Api.DTOs.User;

public class UserPreferencesDto
{
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
}
