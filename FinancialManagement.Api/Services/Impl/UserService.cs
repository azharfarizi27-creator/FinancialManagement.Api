using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.User;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Services.Impl;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        AppDbContext context,
        IFileStorageService fileStorageService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId)
    {
        _logger.LogInformation("Mengambil profil untuk UserId {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        return MapToProfileResponse(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        _logger.LogInformation("Memperbarui profil untuk UserId {UserId}", userId);

        var user = await _userRepository.GetTrackedByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        var name = !string.IsNullOrWhiteSpace(request.FullName) ? request.FullName.Trim() : (request.Name?.Trim() ?? user.FullName);
        var phone = request.PhoneNumber?.Trim() ?? request.Phone?.Trim() ?? user.PhoneNumber;

        user.FullName = name;
        user.PhoneNumber = phone;
        user.Bio = request.Bio?.Trim() ?? user.Bio;
        user.AvatarColor = request.AvatarColor?.Trim() ?? user.AvatarColor;
        user.AvatarIcon = request.AvatarIcon?.Trim() ?? user.AvatarIcon;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Profil berhasil diperbarui untuk UserId {UserId}", userId);

        return MapToProfileResponse(user);
    }

    public async Task<UserProfileResponse> UpdateAvatarAsync(int userId, IFormFile file)
    {
        _logger.LogInformation("Mengunggah foto profil untuk UserId {UserId}", userId);

        var user = await _userRepository.GetTrackedByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        // Delete previous avatar file if exists
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            _fileStorageService.DeleteFile(user.AvatarUrl);
        }

        var newAvatarUrl = await _fileStorageService.SaveFileAsync(file, "avatars");
        user.AvatarUrl = newAvatarUrl;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Foto profil berhasil diperbarui untuk UserId {UserId}: {AvatarUrl}", userId, newAvatarUrl);

        return MapToProfileResponse(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        _logger.LogInformation("Mencoba mengubah password untuk UserId {UserId}", userId);

        var user = await _userRepository.GetTrackedByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword,
            user.PasswordHash);

        if (!isOldPasswordValid)
        {
            _logger.LogWarning("Gagal ganti password: Password saat ini salah untuk UserId {UserId}", userId);
            throw new BadRequestException("Kata sandi saat ini tidak sesuai.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Kata sandi berhasil diperbarui untuk UserId {UserId}", userId);

        return true;
    }

    public async Task<AccountStatsResponse> GetAccountStatsAsync(int userId)
    {
        _logger.LogInformation("Mengambil statistik akun untuk UserId {UserId}", userId);

        var totalWallets = await _context.Wallets.CountAsync(w => w.UserId == userId);
        var totalBalance = await _context.Wallets.Where(w => w.UserId == userId).SumAsync(w => (decimal?)w.Balance) ?? 0;
        var totalCategories = await _context.Categories.CountAsync(c => c.UserId == userId);
        var totalBudgets = await _context.Budgets.CountAsync(b => b.UserId == userId);
        var totalGoals = await _context.SavingsGoals.CountAsync(g => g.UserId == userId);
        var completedGoals = await _context.SavingsGoals.CountAsync(g => g.UserId == userId && g.CurrentAmount >= g.TargetAmount);
        var totalBills = await _context.RecurringBills.CountAsync(b => b.UserId == userId);
        var monthlyBillCommitment = await _context.RecurringBills.Where(b => b.UserId == userId).SumAsync(b => (decimal?)b.Amount) ?? 0;
        var totalTransactions = await _context.Transactions.CountAsync(t => t.UserId == userId);

        return new AccountStatsResponse
        {
            TotalWallets = totalWallets,
            TotalBalance = totalBalance,
            TotalCategories = totalCategories,
            TotalBudgets = totalBudgets,
            TotalGoals = totalGoals,
            CompletedGoals = completedGoals,
            TotalBills = totalBills,
            MonthlyBillCommitment = monthlyBillCommitment,
            TotalTransactions = totalTransactions
        };
    }

    public async Task<UserPreferencesDto> GetPreferencesAsync(int userId)
    {
        _logger.LogInformation("Mengambil preferensi untuk UserId {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        return MapToPreferencesDto(user);
    }

    public async Task<UserPreferencesDto> UpdatePreferencesAsync(int userId, UserPreferencesDto request)
    {
        _logger.LogInformation("Memperbarui preferensi untuk UserId {UserId}", userId);

        var user = await _userRepository.GetTrackedByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        user.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "IDR" : request.Currency.Trim().ToUpper();
        user.NumberFormat = string.IsNullOrWhiteSpace(request.NumberFormat) ? "full" : request.NumberFormat.Trim().ToLower();
        user.Theme = string.IsNullOrWhiteSpace(request.Theme) ? "light" : request.Theme.Trim().ToLower();
        user.Language = string.IsNullOrWhiteSpace(request.Language) ? "id" : request.Language.Trim().ToLower();
        user.DateFormat = string.IsNullOrWhiteSpace(request.DateFormat) ? "DD/MM/YYYY" : request.DateFormat.Trim();

        user.BillReminder = request.BillReminder;
        user.BudgetAlert = request.BudgetAlert;
        user.WeeklyDigest = request.WeeklyDigest;

        user.NotifyGoalMilestone = request.NotifyGoalMilestone;
        user.NotifyBillDue = request.NotifyBillDue;
        user.NotifyBudgetLimit = request.NotifyBudgetLimit;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Preferensi berhasil diperbarui untuk UserId {UserId}", userId);

        return MapToPreferencesDto(user);
    }

    public async Task<UserBackupDataDto> ExportBackupDataAsync(int userId)
    {
        _logger.LogInformation("Membuat data backup lengkap untuk UserId {UserId}", userId);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        var wallets = await _context.Wallets.AsNoTracking().Where(w => w.UserId == userId).ToListAsync();
        var categories = await _context.Categories.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
        var transactions = await _context.Transactions.AsNoTracking()
            .Include(t => t.Wallet)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();

        var budgets = await _context.Budgets.AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        var goals = await _context.SavingsGoals.AsNoTracking()
            .Include(g => g.Deposits)
                .ThenInclude(d => d.Wallet)
            .Where(g => g.UserId == userId)
            .ToListAsync();

        var bills = await _context.RecurringBills.AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Payments)
                .ThenInclude(p => p.Wallet)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        return new UserBackupDataDto
        {
            AppVersion = "1.0",
            ExportedAt = DateTime.UtcNow,
            Profile = new UserProfileBackupDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                AvatarColor = user.AvatarColor,
                AvatarIcon = user.AvatarIcon,
                AvatarUrl = user.AvatarUrl
            },
            Preferences = MapToPreferencesDto(user),
            Wallets = wallets.Select(w => new WalletBackupDto
            {
                Name = w.Name,
                Balance = w.Balance,
                Type = w.Type
            }).ToList(),
            Categories = categories.Select(c => new CategoryBackupDto
            {
                Name = c.Name,
                Type = c.Type
            }).ToList(),
            Transactions = transactions.Select(t => new TransactionBackupDto
            {
                WalletName = t.Wallet?.Name ?? string.Empty,
                CategoryName = t.Category?.Name ?? string.Empty,
                Amount = t.Amount,
                Type = t.Type,
                Description = t.Description,
                ReceiptUrl = t.ReceiptUrl,
                TransactionDate = t.TransactionDate,
                CreatedAt = t.CreatedAt
            }).ToList(),
            Budgets = budgets.Select(b => new BudgetBackupDto
            {
                CategoryName = b.Category?.Name ?? string.Empty,
                LimitAmount = b.LimitAmount,
                Month = b.Month,
                Year = b.Year
            }).ToList(),
            SavingsGoals = goals.Select(g => new SavingsGoalBackupDto
            {
                Title = g.Title,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.CurrentAmount,
                TargetDate = g.TargetDate,
                Category = g.Category,
                Description = g.Description,
                Color = g.Color,
                Icon = g.Icon,
                Deposits = g.Deposits.Select(d => new SavingsGoalDepositBackupDto
                {
                    WalletName = d.Wallet?.Name,
                    Amount = d.Amount,
                    Date = d.Date,
                    Note = d.Note
                }).ToList()
            }).ToList(),
            RecurringBills = bills.Select(b => new RecurringBillBackupDto
            {
                Title = b.Title,
                Amount = b.Amount,
                DueDate = b.DueDate,
                Frequency = b.Frequency,
                CategoryName = b.Category?.Name,
                ReminderDays = b.ReminderDays,
                IsPaidThisCycle = b.IsPaidThisCycle,
                LastPaidDate = b.LastPaidDate,
                Notes = b.Notes,
                Payments = b.Payments.Select(p => new RecurringBillPaymentBackupDto
                {
                    WalletName = p.Wallet?.Name,
                    Amount = p.Amount,
                    PaidDate = p.PaidDate,
                    Note = p.Note
                }).ToList()
            }).ToList()
        };
    }

    public async Task<RestoreBackupResultDto> RestoreBackupDataAsync(int userId, UserBackupDataDto backupData, bool overwrite = false)
    {
        _logger.LogInformation("Memulihkan data backup untuk UserId {UserId} (Overwrite: {Overwrite})", userId, overwrite);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User tidak ditemukan.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // If overwrite is requested, clear existing transactions, budgets, goals, bills
            if (overwrite)
            {
                var existingTx = await _context.Transactions.Where(t => t.UserId == userId).ToListAsync();
                _context.Transactions.RemoveRange(existingTx);

                var existingBudgets = await _context.Budgets.Where(b => b.UserId == userId).ToListAsync();
                _context.Budgets.RemoveRange(existingBudgets);

                var existingGoals = await _context.SavingsGoals.Where(g => g.UserId == userId).ToListAsync();
                _context.SavingsGoals.RemoveRange(existingGoals);

                var existingBills = await _context.RecurringBills.Where(b => b.UserId == userId).ToListAsync();
                _context.RecurringBills.RemoveRange(existingBills);

                await _context.SaveChangesAsync();
            }

            // 1. Update Preferences if available
            if (backupData.Preferences != null)
            {
                user.Currency = string.IsNullOrWhiteSpace(backupData.Preferences.Currency) ? user.Currency : backupData.Preferences.Currency;
                user.NumberFormat = string.IsNullOrWhiteSpace(backupData.Preferences.NumberFormat) ? user.NumberFormat : backupData.Preferences.NumberFormat;
                user.Theme = string.IsNullOrWhiteSpace(backupData.Preferences.Theme) ? user.Theme : backupData.Preferences.Theme;
                user.Language = string.IsNullOrWhiteSpace(backupData.Preferences.Language) ? user.Language : backupData.Preferences.Language;
                user.DateFormat = string.IsNullOrWhiteSpace(backupData.Preferences.DateFormat) ? user.DateFormat : backupData.Preferences.DateFormat;
                user.BillReminder = backupData.Preferences.BillReminder;
                user.BudgetAlert = backupData.Preferences.BudgetAlert;
                user.WeeklyDigest = backupData.Preferences.WeeklyDigest;
                user.NotifyGoalMilestone = backupData.Preferences.NotifyGoalMilestone;
                user.NotifyBillDue = backupData.Preferences.NotifyBillDue;
                user.NotifyBudgetLimit = backupData.Preferences.NotifyBudgetLimit;
            }

            // 2. Wallets mapping
            var existingWallets = await _context.Wallets.Where(w => w.UserId == userId).ToListAsync();
            var walletMap = existingWallets.ToDictionary(w => w.Name.ToLowerInvariant(), w => w);

            int restoredWallets = 0;
            foreach (var wDto in backupData.Wallets)
            {
                var key = wDto.Name.Trim().ToLowerInvariant();
                if (!walletMap.TryGetValue(key, out var wallet))
                {
                    wallet = new Wallet
                    {
                        UserId = userId,
                        Name = wDto.Name.Trim(),
                        Balance = wDto.Balance,
                        Type = string.IsNullOrWhiteSpace(wDto.Type) ? "Cash" : wDto.Type,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Wallets.Add(wallet);
                    await _context.SaveChangesAsync();
                    walletMap[key] = wallet;
                    restoredWallets++;
                }
            }

            // Ensure fallback wallet
            if (!walletMap.Any())
            {
                var defaultWallet = new Wallet
                {
                    UserId = userId,
                    Name = "Dompet Utama",
                    Balance = 0,
                    Type = "Cash",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Wallets.Add(defaultWallet);
                await _context.SaveChangesAsync();
                walletMap[defaultWallet.Name.ToLowerInvariant()] = defaultWallet;
            }

            // 3. Categories mapping
            var existingCategories = await _context.Categories.Where(c => c.UserId == userId).ToListAsync();
            var categoryMap = existingCategories.ToDictionary(c => c.Name.ToLowerInvariant(), c => c);

            int restoredCategories = 0;
            foreach (var cDto in backupData.Categories)
            {
                var key = cDto.Name.Trim().ToLowerInvariant();
                if (!categoryMap.TryGetValue(key, out var category))
                {
                    category = new Category
                    {
                        UserId = userId,
                        Name = cDto.Name.Trim(),
                        Type = string.IsNullOrWhiteSpace(cDto.Type) ? "Expense" : cDto.Type
                    };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                    categoryMap[key] = category;
                    restoredCategories++;
                }
            }

            // Ensure fallback category
            if (!categoryMap.Any())
            {
                var defaultCategory = new Category
                {
                    UserId = userId,
                    Name = "Lain-lain",
                    Type = "Expense"
                };
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
                categoryMap[defaultCategory.Name.ToLowerInvariant()] = defaultCategory;
            }

            var fallbackWallet = walletMap.Values.First();
            var fallbackCategory = categoryMap.Values.First();

            // 4. Transactions
            int restoredTransactions = 0;
            foreach (var tDto in backupData.Transactions)
            {
                var wallet = walletMap.TryGetValue(tDto.WalletName.Trim().ToLowerInvariant(), out var w) ? w : fallbackWallet;
                var category = categoryMap.TryGetValue(tDto.CategoryName.Trim().ToLowerInvariant(), out var c) ? c : fallbackCategory;

                var transactionEntity = new Transaction
                {
                    UserId = userId,
                    WalletId = wallet.Id,
                    CategoryId = category.Id,
                    Amount = tDto.Amount,
                    Type = string.IsNullOrWhiteSpace(tDto.Type) ? "Expense" : tDto.Type,
                    Description = tDto.Description,
                    ReceiptUrl = tDto.ReceiptUrl,
                    TransactionDate = tDto.TransactionDate == default ? DateTime.UtcNow : tDto.TransactionDate,
                    CreatedAt = tDto.CreatedAt == default ? DateTime.UtcNow : tDto.CreatedAt
                };
                _context.Transactions.Add(transactionEntity);
                restoredTransactions++;
            }

            // 5. Budgets
            int restoredBudgets = 0;
            foreach (var bDto in backupData.Budgets)
            {
                var category = categoryMap.TryGetValue(bDto.CategoryName.Trim().ToLowerInvariant(), out var c) ? c : fallbackCategory;

                var budgetEntity = new Budget
                {
                    UserId = userId,
                    CategoryId = category.Id,
                    LimitAmount = bDto.LimitAmount,
                    Month = bDto.Month > 0 && bDto.Month <= 12 ? bDto.Month : DateTime.UtcNow.Month,
                    Year = bDto.Year > 2000 ? bDto.Year : DateTime.UtcNow.Year,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Budgets.Add(budgetEntity);
                restoredBudgets++;
            }

            // 6. Savings Goals & Deposits
            int restoredGoals = 0;
            foreach (var gDto in backupData.SavingsGoals)
            {
                var goal = new SavingsGoal
                {
                    UserId = userId,
                    Title = gDto.Title.Trim(),
                    TargetAmount = gDto.TargetAmount,
                    CurrentAmount = gDto.CurrentAmount,
                    TargetDate = gDto.TargetDate,
                    Category = gDto.Category,
                    Description = gDto.Description,
                    Color = gDto.Color,
                    Icon = gDto.Icon,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var dDto in gDto.Deposits)
                {
                    var depWallet = !string.IsNullOrWhiteSpace(dDto.WalletName) && walletMap.TryGetValue(dDto.WalletName.Trim().ToLowerInvariant(), out var dw)
                        ? dw
                        : fallbackWallet;

                    goal.Deposits.Add(new SavingsGoalDeposit
                    {
                        WalletId = depWallet.Id,
                        Amount = dDto.Amount,
                        Date = dDto.Date == default ? DateTime.UtcNow : dDto.Date,
                        Note = dDto.Note
                    });
                }

                _context.SavingsGoals.Add(goal);
                restoredGoals++;
            }

            // 7. Recurring Bills & Payments
            int restoredBills = 0;
            foreach (var bDto in backupData.RecurringBills)
            {
                var billCategory = !string.IsNullOrWhiteSpace(bDto.CategoryName) && categoryMap.TryGetValue(bDto.CategoryName.Trim().ToLowerInvariant(), out var bc)
                    ? bc
                    : fallbackCategory;

                var bill = new RecurringBill
                {
                    UserId = userId,
                    Title = bDto.Title.Trim(),
                    Amount = bDto.Amount,
                    DueDate = bDto.DueDate == default ? DateTime.UtcNow.AddDays(7) : bDto.DueDate,
                    Frequency = string.IsNullOrWhiteSpace(bDto.Frequency) ? "monthly" : bDto.Frequency,
                    CategoryId = billCategory.Id,
                    ReminderDays = bDto.ReminderDays > 0 ? bDto.ReminderDays : 3,
                    IsPaidThisCycle = bDto.IsPaidThisCycle,
                    LastPaidDate = bDto.LastPaidDate,
                    Notes = bDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var pDto in bDto.Payments)
                {
                    var payWallet = !string.IsNullOrWhiteSpace(pDto.WalletName) && walletMap.TryGetValue(pDto.WalletName.Trim().ToLowerInvariant(), out var pw)
                        ? pw
                        : fallbackWallet;

                    bill.Payments.Add(new RecurringBillPayment
                    {
                        WalletId = payWallet.Id,
                        Amount = pDto.Amount,
                        PaidDate = pDto.PaidDate == default ? DateTime.UtcNow : pDto.PaidDate,
                        Note = pDto.Note
                    });
                }

                _context.RecurringBills.Add(bill);
                restoredBills++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Data backup berhasil dipulihkan untuk UserId {UserId}", userId);

            return new RestoreBackupResultDto
            {
                Success = true,
                Message = "Data backup berhasil dipulihkan.",
                RestoredWalletsCount = restoredWallets,
                RestoredCategoriesCount = restoredCategories,
                RestoredTransactionsCount = restoredTransactions,
                RestoredBudgetsCount = restoredBudgets,
                RestoredGoalsCount = restoredGoals,
                RestoredBillsCount = restoredBills
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Gagal memulihkan data backup untuk UserId {UserId}", userId);
            throw new BadRequestException($"Gagal memulihkan data: {ex.Message}");
        }
    }

    private static UserPreferencesDto MapToPreferencesDto(Models.User user)
    {
        return new UserPreferencesDto
        {
            Currency = string.IsNullOrWhiteSpace(user.Currency) ? "IDR" : user.Currency,
            NumberFormat = string.IsNullOrWhiteSpace(user.NumberFormat) ? "full" : user.NumberFormat,
            Theme = string.IsNullOrWhiteSpace(user.Theme) ? "light" : user.Theme,
            Language = string.IsNullOrWhiteSpace(user.Language) ? "id" : user.Language,
            DateFormat = string.IsNullOrWhiteSpace(user.DateFormat) ? "DD/MM/YYYY" : user.DateFormat,
            BillReminder = user.BillReminder,
            BudgetAlert = user.BudgetAlert,
            WeeklyDigest = user.WeeklyDigest,
            NotifyGoalMilestone = user.NotifyGoalMilestone,
            NotifyBillDue = user.NotifyBillDue,
            NotifyBudgetLimit = user.NotifyBudgetLimit
        };
    }

    private static UserProfileResponse MapToProfileResponse(Models.User user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            AvatarColor = user.AvatarColor,
            AvatarIcon = user.AvatarIcon,
            AvatarUrl = user.AvatarUrl,
            Theme = user.Theme,
            Language = user.Language,
            DateFormat = user.DateFormat,
            CreatedAt = user.CreatedAt
        };
    }
}
