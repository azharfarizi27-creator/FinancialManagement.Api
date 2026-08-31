using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Services.Background;

public class NotificationSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public NotificationSchedulerService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Worker NotificationSchedulerService telah dimulai.");

        // Jeda awal beberapa detik setelah startup agar app inisialisasi tuntas
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan pada loop eksekusi NotificationSchedulerService.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Background Worker NotificationSchedulerService dihentikan.");
    }

    public async Task ProcessScheduledNotificationsAsync(CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Menjalankan pengecekan terjadwal notifikasi tagihan dan anggaran...");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateTime.UtcNow.Date;
        var now = DateTime.UtcNow;

        // 1. Pengecekan Tagihan Jatuh Tempo (H-3, H-1, Hari H)
        var pendingBills = await context.RecurringBills
            .Include(b => b.User)
            .Include(b => b.Category)
            .Where(b => !b.IsPaidThisCycle)
            .ToListAsync(stoppingToken);

        int billNotifCount = 0;

        foreach (var bill in pendingBills)
        {
            if (bill.User == null) continue;

            // Periksa preferensi notifikasi user
            if (!bill.User.BillReminder || !bill.User.NotifyBillDue) continue;

            var billDueDate = bill.DueDate.Date;
            var remainingDays = (int)(billDueDate - today).TotalDays;

            // Cek kondisi H-3, H-1, atau Hari H (0)
            if (remainingDays == 3 || remainingDays == 1 || remainingDays == 0)
            {
                // Cegah notifikasi duplikat pada hari yang sama untuk tagihan ini
                var alreadyNotifiedToday = await context.Notifications.AnyAsync(n =>
                    n.UserId == bill.UserId &&
                    n.Type == "bill" &&
                    n.CreatedAt >= today &&
                    n.Title.Contains(bill.Title),
                    stoppingToken);

                if (!alreadyNotifiedToday)
                {
                    string dueText = remainingDays == 0
                        ? "hari ini"
                        : remainingDays == 1
                            ? "besok (H-1)"
                            : $"dalam 3 hari (H-3, jatuh tempo: {bill.DueDate:dd/MM/yyyy})";

                    var notification = new Notification
                    {
                        UserId = bill.UserId,
                        Title = $"Pengingat Tagihan: {bill.Title}",
                        Message = $"Tagihan '{bill.Title}' sebesar Rp {bill.Amount:N0} akan jatuh tempo {dueText}.",
                        Type = "bill",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Notifications.Add(notification);
                    billNotifCount++;
                }
            }
        }

        // 2. Pengecekan Anggaran Melebihi Batas (>= 80% & >= 100%)
        var activeBudgets = await context.Budgets
            .Include(b => b.User)
            .Include(b => b.Category)
            .Where(b => b.Month == now.Month && b.Year == now.Year && b.LimitAmount > 0)
            .ToListAsync(stoppingToken);

        int budgetNotifCount = 0;

        foreach (var budget in activeBudgets)
        {
            if (budget.User == null || budget.Category == null) continue;

            // Periksa preferensi notifikasi user
            if (!budget.User.BudgetAlert || !budget.User.NotifyBudgetLimit) continue;

            var currentMonthExpense = await context.Transactions
                .Where(t => t.UserId == budget.UserId &&
                            t.CategoryId == budget.CategoryId &&
                            t.Type == "Expense" &&
                            t.TransactionDate.Month == now.Month &&
                            t.TransactionDate.Year == now.Year)
                .SumAsync(t => (decimal?)t.Amount, stoppingToken) ?? 0;

            var usagePercent = (currentMonthExpense / budget.LimitAmount) * 100;

            if (usagePercent >= 100)
            {
                var notified100 = await context.Notifications.AnyAsync(n =>
                    n.UserId == budget.UserId &&
                    n.Type == "budget" &&
                    n.CreatedAt.Month == now.Month &&
                    n.CreatedAt.Year == now.Year &&
                    n.Message.Contains("100%") &&
                    n.Title.Contains(budget.Category.Name),
                    stoppingToken);

                if (!notified100)
                {
                    var notification = new Notification
                    {
                        UserId = budget.UserId,
                        Title = $"Peringatan Anggaran: {budget.Category.Name}",
                        Message = $"Anggaran kategori '{budget.Category.Name}' telah melampaui batas 100%! (Terpakai: Rp {currentMonthExpense:N0} dari Limit: Rp {budget.LimitAmount:N0}).",
                        Type = "budget",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Notifications.Add(notification);
                    budgetNotifCount++;
                }
            }
            else if (usagePercent >= 80)
            {
                var notified80 = await context.Notifications.AnyAsync(n =>
                    n.UserId == budget.UserId &&
                    n.Type == "budget" &&
                    n.CreatedAt.Month == now.Month &&
                    n.CreatedAt.Year == now.Year &&
                    n.Message.Contains("80%") &&
                    n.Title.Contains(budget.Category.Name),
                    stoppingToken);

                if (!notified80)
                {
                    var notification = new Notification
                    {
                        UserId = budget.UserId,
                        Title = $"Perhatian Anggaran: {budget.Category.Name}",
                        Message = $"Anggaran kategori '{budget.Category.Name}' telah mencapai {usagePercent:F0}% (Terpakai: Rp {currentMonthExpense:N0} dari Limit: Rp {budget.LimitAmount:N0}).",
                        Type = "budget",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Notifications.Add(notification);
                    budgetNotifCount++;
                }
            }
        }

        if (billNotifCount > 0 || budgetNotifCount > 0)
        {
            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Berhasil membuat {BillCount} notifikasi tagihan dan {BudgetCount} notifikasi batas anggaran.",
                billNotifCount, budgetNotifCount);
        }
        else
        {
            _logger.LogInformation("Pengecekan selesai. Tidak ada notifikasi baru yang perlu dibuat.");
        }
    }
}
