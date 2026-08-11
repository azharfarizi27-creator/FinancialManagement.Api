using FinancialManagement.Api.DTOs.Dashboard;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<FinancialSummaryResponse> GetSummaryAsync(
        int userId)
    {
        var totalBalance =
            await _repository.GetTotalBalanceAsync(userId);

        var totalIncome =
            await _repository.GetTotalIncomeAsync(userId);

        var totalExpense =
            await _repository.GetTotalExpenseAsync(userId);

        var netBalance = totalIncome - totalExpense;

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
        var transactions =
            await _repository.GetRecentTransactionsAsync(
                userId,
                limit);

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

        return new MonthlySummaryResponse
        {
            Month = month,
            Year = year,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense
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
        var data =
            await _repository.GetCategorySummaryAsync(
                userId,
                month,
                year,
                type);

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
        var data =
            await _repository.GetWalletSummaryAsync(userId);

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