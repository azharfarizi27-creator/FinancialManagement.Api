using FinancialManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Budget> Budgets => Set<Budget>();

    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();

    public DbSet<SavingsGoalDeposit> SavingsGoalDeposits => Set<SavingsGoalDeposit>();

    public DbSet<RecurringBill> RecurringBills => Set<RecurringBill>();

    public DbSet<RecurringBillPayment> RecurringBillPayments => Set<RecurringBillPayment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<OtpToken> OtpTokens => Set<OtpToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // OtpToken index
        modelBuilder.Entity<OtpToken>()
            .HasIndex(x => new { x.Email, x.Purpose });

        modelBuilder.Entity<OtpToken>()
            .HasIndex(x => x.ExpiresAt);

        // User
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // User -> Wallet
        modelBuilder.Entity<Wallet>()
            .HasOne(x => x.User)
            .WithMany(x => x.Wallets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Category
        modelBuilder.Entity<Category>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Notification
        modelBuilder.Entity<Notification>()
            .HasOne(x => x.User)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> PasswordResetToken
        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(x => x.User)
            .WithMany(x => x.PasswordResetTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(x => x.Token);

        // User -> Transaction
        modelBuilder.Entity<Transaction>()
            .HasOne(x => x.User)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Wallet -> Transaction
        modelBuilder.Entity<Transaction>()
            .HasOne(x => x.Wallet)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category -> Transaction
        modelBuilder.Entity<Transaction>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> Budget
        modelBuilder.Entity<Budget>()
            .HasOne(x => x.User)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> Budget
        modelBuilder.Entity<Budget>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> SavingsGoal
        modelBuilder.Entity<SavingsGoal>()
            .HasOne(x => x.User)
            .WithMany(x => x.SavingsGoals)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // SavingsGoal -> SavingsGoalDeposit
        modelBuilder.Entity<SavingsGoalDeposit>()
            .HasOne(x => x.SavingsGoal)
            .WithMany(x => x.Deposits)
            .HasForeignKey(x => x.SavingsGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Wallet -> SavingsGoalDeposit
        modelBuilder.Entity<SavingsGoalDeposit>()
            .HasOne(x => x.Wallet)
            .WithMany()
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> RecurringBill
        modelBuilder.Entity<RecurringBill>()
            .HasOne(x => x.User)
            .WithMany(x => x.RecurringBills)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> RecurringBill
        modelBuilder.Entity<RecurringBill>()
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // RecurringBill -> RecurringBillPayment
        modelBuilder.Entity<RecurringBillPayment>()
            .HasOne(x => x.RecurringBill)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.RecurringBillId)
            .OnDelete(DeleteBehavior.Cascade);

        // Wallet -> RecurringBillPayment
        modelBuilder.Entity<RecurringBillPayment>()
            .HasOne(x => x.Wallet)
            .WithMany()
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal precision
        modelBuilder.Entity<Wallet>()
            .Property(x => x.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Budget>()
            .Property(x => x.LimitAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoal>()
            .Property(x => x.TargetAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoal>()
            .Property(x => x.CurrentAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoalDeposit>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RecurringBill>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RecurringBillPayment>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);
    }
}