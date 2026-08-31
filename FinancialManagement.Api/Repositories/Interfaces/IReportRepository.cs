using FinancialManagement.Api.DTOs.Report;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IReportRepository
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
