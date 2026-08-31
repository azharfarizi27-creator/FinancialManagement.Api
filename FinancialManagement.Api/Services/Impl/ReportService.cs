using FinancialManagement.Api.DTOs.Report;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepository,
        ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _logger = logger;
    }

    public async Task<ReportSummaryResponse> GetSummaryAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        int? walletId)
    {
        _logger.LogInformation("Mengambil ringkasan laporan untuk UserId {UserId}, StartDate {Start}, EndDate {End}, WalletId {WalletId}",
            userId, startDate, endDate, walletId);

        return await _reportRepository.GetSummaryAsync(userId, startDate, endDate, walletId);
    }

    public async Task<CashflowReportResponse> GetCashflowTrendAsync(
        int userId,
        string period,
        int year,
        int? walletId)
    {
        _logger.LogInformation("Mengambil tren arus kas untuk UserId {UserId}, Period {Period}, Year {Year}, WalletId {WalletId}",
            userId, period, year, walletId);

        if (year < 2000 || year > 2100)
        {
            year = DateTime.UtcNow.Year;
        }

        return await _reportRepository.GetCashflowTrendAsync(userId, period, year, walletId);
    }

    public async Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(
        int userId,
        string type,
        DateTime? startDate,
        DateTime? endDate,
        int? walletId)
    {
        _logger.LogInformation("Mengambil proporsi kategori untuk UserId {UserId}, Type {Type}, StartDate {Start}, EndDate {End}, WalletId {WalletId}",
            userId, type, startDate, endDate, walletId);

        var normalizedType = type.Equals("Income", StringComparison.OrdinalIgnoreCase) ? "Income" : "Expense";

        return await _reportRepository.GetCategoryBreakdownAsync(userId, normalizedType, startDate, endDate, walletId);
    }
}
