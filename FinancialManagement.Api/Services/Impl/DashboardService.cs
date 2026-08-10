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
}