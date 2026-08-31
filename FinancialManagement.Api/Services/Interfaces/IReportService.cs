using FinancialManagement.Api.DTOs.Report;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IReportService
{
    Task<ReportSummaryResponse> GetSummaryAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        int? walletId);

    Task<CashflowReportResponse> GetCashflowTrendAsync(
        int userId,
        string period,
        int year,
        int? walletId);

    Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(
        int userId,
        string type,
        DateTime? startDate,
        DateTime? endDate,
        int? walletId);
}
