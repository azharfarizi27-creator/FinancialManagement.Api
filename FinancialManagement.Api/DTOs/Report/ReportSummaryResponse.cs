namespace FinancialManagement.Api.DTOs.Report;

public class ReportSummaryResponse
{
    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetSavings => TotalIncome - TotalExpense;

    public double SavingsRate => TotalIncome > 0 ? (double)Math.Round(((TotalIncome - TotalExpense) / TotalIncome) * 100, 2) : 0;

    public decimal AverageDailyExpense { get; set; }

    public int TotalTransactions { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
