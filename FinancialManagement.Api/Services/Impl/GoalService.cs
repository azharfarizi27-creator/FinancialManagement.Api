using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.Goal;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Services.Impl;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<GoalService> _logger;

    public GoalService(
        IGoalRepository goalRepository,
        IWalletRepository walletRepository,
        AppDbContext context,
        ILogger<GoalService> logger)
    {
        _goalRepository = goalRepository;
        _walletRepository = walletRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<List<GoalResponse>> GetAllAsync(int userId)
    {
        _logger.LogInformation("Mengambil semua target tabungan untuk UserId {UserId}", userId);

        var goals = await _goalRepository.GetByUserIdAsync(userId);

        return goals.Select(MapToResponse).ToList();
    }

    public async Task<GoalResponse?> GetByIdAsync(int id, int userId)
    {
        _logger.LogInformation("Mengambil target tabungan Id {GoalId} untuk UserId {UserId}", id, userId);

        var goal = await _goalRepository.GetByIdAsync(id, userId);
        if (goal == null)
        {
            return null;
        }

        return MapToResponse(goal);
    }

    public async Task<GoalResponse> CreateAsync(CreateGoalRequest request, int userId)
    {
        _logger.LogInformation("Membuat target tabungan baru '{Title}' (Target: {Target}) untuk UserId {UserId}",
            request.Title, request.TargetAmount, userId);

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var goal = new SavingsGoal
            {
                UserId = userId,
                Title = request.Title.Trim(),
                TargetAmount = request.TargetAmount,
                CurrentAmount = request.CurrentAmount,
                TargetDate = request.TargetDate,
                Category = request.Category?.Trim(),
                Color = request.Color?.Trim(),
                Icon = request.Icon?.Trim(),
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _goalRepository.CreateAsync(goal);

            if (request.CurrentAmount > 0)
            {
                var deposit = new SavingsGoalDeposit
                {
                    SavingsGoalId = goal.Id,
                    WalletId = request.InitialDepositWalletId,
                    Amount = request.CurrentAmount,
                    Date = DateTime.UtcNow,
                    Note = "Setoran Awal"
                };

                if (request.InitialDepositWalletId.HasValue)
                {
                    var wallet = await _walletRepository.GetTrackedByIdAsync(request.InitialDepositWalletId.Value, userId);
                    if (wallet != null)
                    {
                        if (wallet.Balance < request.CurrentAmount)
                        {
                            throw new BadRequestException($"Saldo dompet '{wallet.Name}' tidak mencukupi untuk setoran awal.");
                        }

                        wallet.Balance -= request.CurrentAmount;

                        var category = await _context.Categories.FirstOrDefaultAsync(c => c.UserId == userId);
                        var tx = new Transaction
                        {
                            UserId = userId,
                            WalletId = wallet.Id,
                            CategoryId = category?.Id ?? 1,
                            Amount = request.CurrentAmount,
                            Type = "Expense",
                            Description = $"Setoran Awal Tabungan: {goal.Title}",
                            TransactionDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Transactions.Add(tx);
                    }
                }

                _context.SavingsGoalDeposits.Add(deposit);
                await _context.SaveChangesAsync();
            }

            await dbTransaction.CommitAsync();

            _logger.LogInformation("Target tabungan Id {GoalId} berhasil dibuat untuk UserId {UserId}", goal.Id, userId);

            var createdGoal = await _goalRepository.GetByIdAsync(goal.Id, userId);
            return MapToResponse(createdGoal ?? goal);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Gagal membuat target tabungan untuk UserId {UserId}", userId);
            throw;
        }
    }

    public async Task<GoalResponse?> UpdateAsync(int id, UpdateGoalRequest request, int userId)
    {
        _logger.LogInformation("Memperbarui target tabungan Id {GoalId} untuk UserId {UserId}", id, userId);

        var goal = await _goalRepository.GetTrackedByIdAsync(id, userId);
        if (goal == null)
        {
            return null;
        }

        goal.Title = request.Title.Trim();
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = request.TargetDate;
        goal.Category = request.Category?.Trim();
        goal.Color = request.Color?.Trim();
        goal.Icon = request.Icon?.Trim();
        goal.Description = request.Description?.Trim();

        await _goalRepository.UpdateAsync(goal);

        _logger.LogInformation("Target tabungan Id {GoalId} berhasil diperbarui", id);

        var updatedGoal = await _goalRepository.GetByIdAsync(id, userId);
        return MapToResponse(updatedGoal ?? goal);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        _logger.LogInformation("Menghapus target tabungan Id {GoalId} untuk UserId {UserId}", id, userId);

        var goal = await _goalRepository.GetTrackedByIdAsync(id, userId);
        if (goal == null)
        {
            return false;
        }

        await _goalRepository.DeleteAsync(goal);

        _logger.LogInformation("Target tabungan Id {GoalId} berhasil dihapus", id);

        return true;
    }

    public async Task<GoalResponse> DepositAsync(int id, DepositGoalRequest request, int userId)
    {
        _logger.LogInformation("Menambah setoran tabungan Rp {Amount} pada GoalId {GoalId} untuk UserId {UserId}",
            request.Amount, id, userId);

        if (request.Amount <= 0)
        {
            throw new BadRequestException("Nominal setoran harus lebih besar dari 0.");
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var goal = await _goalRepository.GetTrackedByIdAsync(id, userId);
            if (goal == null)
            {
                throw new NotFoundException("Target tabungan tidak ditemukan.");
            }

            var depositDate = request.Date ?? DateTime.UtcNow;

            if (request.WalletId.HasValue)
            {
                var wallet = await _walletRepository.GetTrackedByIdAsync(request.WalletId.Value, userId);
                if (wallet == null)
                {
                    throw new NotFoundException("Dompet sumber dana tidak ditemukan.");
                }

                if (wallet.Balance < request.Amount)
                {
                    throw new BadRequestException($"Saldo dompet '{wallet.Name}' (Rp {wallet.Balance:N0}) tidak mencukupi untuk setoran sebesar Rp {request.Amount:N0}.");
                }

                wallet.Balance -= request.Amount;

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.UserId == userId && (c.Name == "Tabungan" || c.Name == "Investasi" || c.Name == "Lainnya"));

                if (category == null)
                {
                    category = await _context.Categories.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (category == null)
                    {
                        category = new Category
                        {
                            UserId = userId,
                            Name = "Tabungan",
                            Type = "Expense"
                        };
                        _context.Categories.Add(category);
                        await _context.SaveChangesAsync();
                    }
                }

                var noteText = !string.IsNullOrWhiteSpace(request.Note) ? $" ({request.Note.Trim()})" : "";
                var tx = new Transaction
                {
                    UserId = userId,
                    WalletId = wallet.Id,
                    CategoryId = category.Id,
                    Amount = request.Amount,
                    Type = "Expense",
                    Description = $"Setor Tabungan: {goal.Title}{noteText}",
                    TransactionDate = depositDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(tx);
            }

            goal.CurrentAmount += request.Amount;

            var deposit = new SavingsGoalDeposit
            {
                SavingsGoalId = goal.Id,
                WalletId = request.WalletId,
                Amount = request.Amount,
                Date = depositDate,
                Note = !string.IsNullOrWhiteSpace(request.Note) ? request.Note.Trim() : "Setoran Tabungan"
            };

            _context.SavingsGoalDeposits.Add(deposit);

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            _logger.LogInformation("Setoran Rp {Amount} berhasil ditambahkan ke GoalId {GoalId}. Saldo sekarang: Rp {CurrentAmount}",
                request.Amount, goal.Id, goal.CurrentAmount);

            var updatedGoal = await _goalRepository.GetByIdAsync(goal.Id, userId);
            return MapToResponse(updatedGoal ?? goal);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Gagal memproses setoran tabungan untuk GoalId {GoalId}, UserId {UserId}", id, userId);
            throw;
        }
    }

    private static GoalResponse MapToResponse(SavingsGoal goal)
    {
        var deposits = (goal.Deposits ?? Enumerable.Empty<SavingsGoalDeposit>())
            .OrderByDescending(d => d.Date)
            .Select(d => new GoalDepositResponse
            {
                Id = d.Id,
                SavingsGoalId = d.SavingsGoalId,
                WalletId = d.WalletId,
                WalletName = d.Wallet?.Name,
                Amount = d.Amount,
                Date = d.Date,
                Note = d.Note
            })
            .ToList();

        return new GoalResponse
        {
            Id = goal.Id,
            Title = goal.Title,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            TargetDate = goal.TargetDate,
            Category = goal.Category,
            Color = goal.Color,
            Icon = goal.Icon,
            Description = goal.Description,
            CreatedAt = goal.CreatedAt,
            Deposits = deposits,
            History = deposits
        };
    }
}
