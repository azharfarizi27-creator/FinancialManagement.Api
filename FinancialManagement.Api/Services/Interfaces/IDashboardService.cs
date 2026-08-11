using FinancialManagement.Api.DTOs.Dashboard;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<FinancialSummaryResponse> GetSummaryAsync(int userId);

    Task<List<RecentTransactionResponse>> GetRecentTransactionsAsync(
        int userId,
        int limit = 5);

    Task<MonthlySummaryResponse> GetMonthlySummaryAsync(
    int userId,
    int month,
    int year);

    Task<List<CategorySummaryResponse>> GetCategorySummaryAsync(
        int userId,
        int month,
        int year,
        string type);

    Task<List<WalletSummaryResponse>> GetWalletSummaryAsync(
        int userId);
}