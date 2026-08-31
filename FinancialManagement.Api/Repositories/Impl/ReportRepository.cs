using System.Globalization;
using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.Report;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReportSummaryResponse> GetSummaryAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        int? walletId)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
        {
            var startOfDay = startDate.Value.Date;
            query = query.Where(t => t.TransactionDate >= startOfDay);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.TransactionDate <= endOfDay);
        }

        if (walletId.HasValue)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        var totalIncome = await query
            .Where(t => t.Type == "Income")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var totalExpense = await query
            .Where(t => t.Type == "Expense")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var totalTransactions = await query.CountAsync();

        decimal avgDailyExpense = 0;
        if (startDate.HasValue && endDate.HasValue && totalExpense > 0)
        {
            var days = Math.Max(1, (int)(endDate.Value.Date - startDate.Value.Date).TotalDays + 1);
            avgDailyExpense = Math.Round(totalExpense / days, 2);
        }
        else if (totalExpense > 0)
        {
            // Default 30 hari jika range tidak ditentukan
            avgDailyExpense = Math.Round(totalExpense / 30, 2);
        }

        return new ReportSummaryResponse
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            AverageDailyExpense = avgDailyExpense,
            TotalTransactions = totalTransactions,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<CashflowReportResponse> GetCashflowTrendAsync(
        int userId,
        string period,
        int year,
        int? walletId)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.TransactionDate.Year == year);

        if (walletId.HasValue)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        var monthlyData = await query
            .GroupBy(t => new { t.TransactionDate.Month, t.Type })
            .Select(g => new
            {
                Month = g.Key.Month,
                Type = g.Key.Type,
                Total = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        var monthNames = new[]
        {
            "Jan", "Feb", "Mar", "Apr", "Mei", "Jun",
            "Jul", "Agu", "Sep", "Okt", "Nov", "Des"
        };

        var series = new List<CashflowPointResponse>();
        for (int m = 1; m <= 12; m++)
        {
            var income = monthlyData.FirstOrDefault(x => x.Month == m && x.Type == "Income")?.Total ?? 0;
            var expense = monthlyData.FirstOrDefault(x => x.Month == m && x.Type == "Expense")?.Total ?? 0;

            series.Add(new CashflowPointResponse
            {
                Period = $"{year}-{m:D2}",
                Label = monthNames[m - 1],
                Income = income,
                Expense = expense
            });
        }

        return new CashflowReportResponse
        {
            GroupBy = "monthly",
            Year = year,
            Series = series
        };
    }

    public async Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(
        int userId,
        string type,
        DateTime? startDate,
        DateTime? endDate,
        int? walletId)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Type == type);

        if (startDate.HasValue)
        {
            var startOfDay = startDate.Value.Date;
            query = query.Where(t => t.TransactionDate >= startOfDay);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.TransactionDate <= endOfDay);
        }

        if (walletId.HasValue)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        var grouped = await query
            .GroupBy(t => new { t.CategoryId, t.Category.Name })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalAmount = g.Sum(t => t.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync();

        var totalAll = grouped.Sum(x => x.TotalAmount);

        var categories = grouped.Select(g => new CategoryBreakdownItem
        {
            CategoryId = g.CategoryId,
            CategoryName = g.CategoryName,
            Type = type,
            TotalAmount = g.TotalAmount,
            Percentage = totalAll > 0 ? (double)Math.Round((g.TotalAmount / totalAll) * 100, 2) : 0,
            TransactionCount = g.Count
        }).ToList();

        return new CategoryBreakdownResponse
        {
            Type = type,
            TotalAmount = totalAll,
            StartDate = startDate,
            EndDate = endDate,
            Categories = categories
        };
    }
}
