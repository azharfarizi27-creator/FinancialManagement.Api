using FinancialManagement.Api.DTOs.Dashboard;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<FinancialSummaryResponse> GetSummaryAsync(int userId);

    Task<List<RecentTransactionResponse>> GetRecentTransactionsAsync(
        int userId,
        int limit = 5);
}