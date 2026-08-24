using FinancialManagement.Api.DTOs.Dashboard;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IDashboardRepository repository,
        ILogger<DashboardService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<FinancialSummaryResponse> GetSummaryAsync(
        int userId)
    {
        _logger.LogInformation("Mengambil ringkasan keuangan (Financial Summary) untuk UserId: {UserId}", userId);

        var totalBalance =
            await _repository.GetTotalBalanceAsync(userId);

        var totalIncome =
            await _repository.GetTotalIncomeAsync(userId);

        var totalExpense =
            await _repository.GetTotalExpenseAsync(userId);

        var netBalance = totalIncome - totalExpense;

        _logger.LogInformation(
            "Ringkasan keuangan untuk UserId {UserId}: Balance {TotalBalance}, Income {TotalIncome}, Expense {TotalExpense}, Net {NetBalance}",
            userId, totalBalance, totalIncome, totalExpense, netBalance);

        return new FinancialSummaryResponse
        {
            TotalBalance = totalBalance,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = netBalance
        };
    }

    public async Task<List<RecentTransactionResponse>>
        GetRecentTransactionsAsync(
            int userId,
            int limit = 5)
    {
        _logger.LogInformation("Mengambil {Limit} transaksi terbaru untuk UserId: {UserId}", limit, userId);

        var transactions =
            await _repository.GetRecentTransactionsAsync(
                userId,
                limit);

        _logger.LogInformation("Ditemukan {Count} transaksi terbaru untuk UserId: {UserId}", transactions.Count, userId);

        return transactions
            .Select(transaction => new RecentTransactionResponse
            {
                Id = transaction.Id,
                WalletName = transaction.Wallet.Name,
                CategoryName = transaction.Category.Name,
                Amount = transaction.Amount,
                Type = transaction.Type,
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate
            })
            .ToList();
    }

    // =========================
    // MONTHLY SUMMARY
    // =========================

    public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(
        int userId,
        int month,
        int year)
    {
        _logger.LogInformation("Mengambil ringkasan bulanan periode {Month}/{Year} untuk UserId: {UserId}",
            month, year, userId);

        var totalIncome =
            await _repository.GetMonthlyIncomeAsync(
                userId,
                month,
                year);

        var totalExpense =
            await _repository.GetMonthlyExpenseAsync(
                userId,
                month,
                year);

        var netBalance = totalIncome - totalExpense;

        _logger.LogInformation(
            "Ringkasan bulanan {Month}/{Year} untuk UserId {UserId}: Income {TotalIncome}, Expense {TotalExpense}, Net {NetBalance}",
            month, year, userId, totalIncome, totalExpense, netBalance);

        return new MonthlySummaryResponse
        {
            Month = month,
            Year = year,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = netBalance
        };
    }

    // =========================
    // CATEGORY SUMMARY
    // =========================

    public async Task<List<CategorySummaryResponse>>
        GetCategorySummaryAsync(
            int userId,
            int month,
            int year,
            string type)
    {
        _logger.LogInformation("Mengambil ringkasan kategori (Type: {Type}) periode {Month}/{Year} untuk UserId: {UserId}",
            type, month, year, userId);

        var data =
            await _repository.GetCategorySummaryAsync(
                userId,
                month,
                year,
                type);

        _logger.LogInformation("Ditemukan {Count} kategori ringkasan untuk UserId: {UserId}", data.Count, userId);

        return data
            .Select(category => new CategorySummaryResponse
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                TotalAmount = category.TotalAmount,
                Type = category.Type
            })
            .ToList();
    }

    // =========================
    // WALLET SUMMARY
    // =========================

    public async Task<List<WalletSummaryResponse>>
        GetWalletSummaryAsync(int userId)
    {
        _logger.LogInformation("Mengambil ringkasan per dompet (Wallet Summary) untuk UserId: {UserId}", userId);

        var data =
            await _repository.GetWalletSummaryAsync(userId);

        _logger.LogInformation("Ditemukan {Count} ringkasan dompet untuk UserId: {UserId}", data.Count, userId);

        return data
            .Select(wallet => new WalletSummaryResponse
            {
                WalletId = wallet.WalletId,
                WalletName = wallet.WalletName,
                Type = wallet.Type,
                Balance = wallet.Balance,
                TotalIncome = wallet.TotalIncome,
                TotalExpense = wallet.TotalExpense,
                NetBalance = wallet.NetBalance
            })
            .ToList();
    }
}