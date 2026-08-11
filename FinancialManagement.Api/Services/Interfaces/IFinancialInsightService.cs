using FinancialManagement.Api.DTOs.Dashboard;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IFinancialInsightService
{
    Task<List<FinancialInsightResponse>> GetInsightsAsync(
        int userId);
}