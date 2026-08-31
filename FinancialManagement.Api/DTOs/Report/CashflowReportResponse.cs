namespace FinancialManagement.Api.DTOs.Report;

public class CashflowPointResponse
{
    public string Period { get; set; } = string.Empty; // e.g. "Jan 2026", "2026-08-01", etc.

    public string Label { get; set; } = string.Empty;

    public decimal Income { get; set; }

    public decimal Expense { get; set; }

    public decimal Net => Income - Expense;
}

public class CashflowReportResponse
{
    public string GroupBy { get; set; } = "monthly"; // "daily", "monthly", "yearly"

    public int Year { get; set; }

    public List<CashflowPointResponse> Series { get; set; } = new();
}
