using FinancialManagement.Api.DTOs.Dashboard;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class FinancialInsightService : IFinancialInsightService
{
    private readonly IFinancialInsightRepository _repository;

    public FinancialInsightService(
        IFinancialInsightRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FinancialInsightResponse>> GetInsightsAsync(
        int userId)
    {
        var insights = new List<FinancialInsightResponse>();

        var now = DateTime.UtcNow;

        var month = now.Month;
        var year = now.Year;

        // ==========================================
        // 1. CEK INCOME BULAN INI
        // ==========================================

        var currentIncome =
            await _repository.GetCurrentMonthIncomeAsync(
                userId,
                month,
                year);

        if (currentIncome == 0)
        {
            insights.Add(new FinancialInsightResponse
            {
                Type = "Info",
                Title = "Belum ada pemasukan",
                Message =
                    "Belum ada transaksi pemasukan pada bulan ini."
            });
        }

        // ==========================================
        // 2. CEK EXPENSE BULAN INI
        // ==========================================

        var currentExpense =
            await _repository.GetCurrentMonthExpenseAsync(
                userId,
                month,
                year);

        if (currentExpense == 0)
        {
            insights.Add(new FinancialInsightResponse
            {
                Type = "Info",
                Title = "Belum ada pengeluaran",
                Message =
                    "Belum ada transaksi pengeluaran pada bulan ini."
            });
        }

        // ==========================================
        // 3. BANDINGKAN DENGAN BULAN SEBELUMNYA
        // ==========================================

        var previousExpense =
            await _repository.GetPreviousMonthExpenseAsync(
                userId,
                month,
                year);

        if (previousExpense > 0 &&
            currentExpense > previousExpense)
        {
            var increase =
                ((currentExpense - previousExpense)
                / previousExpense) * 100;

            insights.Add(new FinancialInsightResponse
            {
                Type = "Warning",
                Title = "Pengeluaran meningkat",
                Message =
                    $"Pengeluaran bulan ini meningkat {increase:F1}% dibandingkan bulan sebelumnya."
            });
        }

        // ==========================================
        // 4. CEK BUDGET
        // ==========================================

        var budgets =
            await _repository.GetActiveBudgetsAsync(userId);

        foreach (var budget in budgets)
        {
            var usedAmount =
                await _repository.GetCategoryMonthlyExpenseAsync(
                    userId,
                    budget.CategoryId,
                    budget.Month,
                    budget.Year);

            if (budget.LimitAmount <= 0)
            {
                continue;
            }

            var percentage =
                (usedAmount / budget.LimitAmount) * 100;

            if (percentage >= 100)
            {
                insights.Add(new FinancialInsightResponse
                {
                    Type = "Danger",
                    Title = "Budget terlampaui",
                    Message =
                        $"Budget {budget.Category.Name} sudah terlampaui."
                });
            }
            else if (percentage >= 80)
            {
                insights.Add(new FinancialInsightResponse
                {
                    Type = "Warning",
                    Title = "Budget hampir habis",
                    Message =
                        $"Budget {budget.Category.Name} sudah terpakai {percentage:F1}%."
                });
            }
        }

        // ==========================================
        // 5. KATEGORI PENGELUARAN TERBESAR
        // ==========================================

        var expenses =
            await _repository.GetCurrentMonthExpensesAsync(
                userId,
                month,
                year);

        if (expenses.Count > 0)
        {
            var highestCategory = expenses
                .GroupBy(transaction => new
                {
                    transaction.CategoryId,
                    transaction.Category.Name
                })
                .Select(group => new
                {
                    CategoryName = group.Key.Name,
                    TotalAmount = group.Sum(
                        transaction => transaction.Amount)
                })
                .OrderByDescending(item => item.TotalAmount)
                .First();

            insights.Add(new FinancialInsightResponse
            {
                Type = "Info",
                Title = "Pengeluaran terbesar",
                Message =
                    $"Kategori pengeluaran terbesar bulan ini adalah {highestCategory.CategoryName} sebesar Rp{highestCategory.TotalAmount:N0}."
            });
        }

        // ==========================================
        // 6. EXPENSE LEBIH BESAR DARI INCOME
        // ==========================================

        if (currentExpense > currentIncome &&
            currentExpense > 0)
        {
            insights.Add(new FinancialInsightResponse
            {
                Type = "Danger",
                Title = "Pengeluaran lebih besar dari pemasukan",
                Message =
                    "Total pengeluaran bulan ini lebih besar daripada total pemasukan."
            });
        }

        return insights;
    }
}