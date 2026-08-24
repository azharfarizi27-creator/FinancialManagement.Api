using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.Common;
using FinancialManagement.Api.DTOs.Transaction;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;

namespace FinancialManagement.Api.Services.Impl;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        ICategoryRepository categoryRepository,
        AppDbContext context,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _categoryRepository = categoryRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResponse<TransactionResponse>> GetAllAsync(
        int userId,
        int page = 1,
        int pageSize = 10,
        string? type = null,
        int? categoryId = null,
        int? walletId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        _logger.LogInformation(
            "Mengambil daftar transaksi: UserId {UserId}, Page {Page}, PageSize {PageSize}, Type {Type}, CategoryId {CategoryId}, WalletId {WalletId}, StartDate {StartDate}, EndDate {EndDate}",
            userId, page, pageSize, type, categoryId, walletId, startDate, endDate);

        var totalItems =
            await _transactionRepository.CountByUserIdAsync(
                userId,
                type,
                categoryId,
                walletId,
                startDate,
                endDate);

        var skip = (page - 1) * pageSize;

        var transactions =
            await _transactionRepository.GetPagedByUserIdAsync(
                userId,
                skip,
                pageSize,
                type,
                categoryId,
                walletId,
                startDate,
                endDate);

        var totalPages =
            (int)Math.Ceiling(
                totalItems / (double)pageSize);

        _logger.LogInformation("Ditemukan {TotalItems} transaksi ({TotalPages} halaman) untuk UserId: {UserId}",
            totalItems, totalPages, userId);

        return new PagedResponse<TransactionResponse>
        {
            Data = transactions
                .Select(MapToResponse)
                .ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<TransactionResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mengambil transaksi Id {TransactionId} untuk UserId: {UserId}", id, userId);

        var transaction =
            await _transactionRepository.GetByIdAsync(id, userId);

        if (transaction == null)
        {
            _logger.LogWarning("Transaksi Id {TransactionId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        return MapToResponse(transaction);
    }

    public async Task<TransactionResponse?> CreateAsync(
        CreateTransactionRequest request,
        int userId)
    {
        _logger.LogInformation(
            "Memulai pembuatan transaksi: UserId {UserId}, WalletId {WalletId}, CategoryId {CategoryId}, Amount {Amount}, Type {Type}",
            userId, request.WalletId, request.CategoryId, request.Amount, request.Type);

        var wallet = await _walletRepository.GetByIdAsync(
            request.WalletId,
            userId);

        if (wallet == null)
        {
            _logger.LogWarning("Gagal membuat transaksi: Dompet Id {WalletId} tidak ditemukan atau bukan milik UserId {UserId}",
                request.WalletId, userId);
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            _logger.LogWarning("Gagal membuat transaksi: Kategori Id {CategoryId} tidak ditemukan atau bukan milik UserId {UserId}",
                request.CategoryId, userId);
            return null;
        }

        if (request.Amount <= 0)
        {
            _logger.LogWarning("Gagal membuat transaksi: Nominal Amount {Amount} tidak valid", request.Amount);
            return null;
        }

        if (request.Type != "Income" && request.Type != "Expense")
        {
            _logger.LogWarning("Gagal membuat transaksi: Tipe transaksi {Type} tidak valid", request.Type);
            return null;
        }

        // Jalankan mutasi dan insert transaksi dalam Database Transaction Atomic
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var previousBalance = wallet.Balance;

            if (request.Type == "Income")
            {
                wallet.Balance += request.Amount;
            }
            else
            {
                wallet.Balance -= request.Amount;
            }

            _logger.LogInformation("Saldo dompet Id {WalletId} dimutasi: {PrevBalance} -> {NewBalance}",
                wallet.Id, previousBalance, wallet.Balance);

            var transaction = new Transaction
            {
                UserId = userId,
                WalletId = request.WalletId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Type = request.Type,
                Description = request.Description?.Trim(),
                TransactionDate = request.TransactionDate,
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.CreateAsync(transaction);
            await _walletRepository.UpdateAsync(wallet);

            await dbTransaction.CommitAsync();

            _logger.LogInformation("Transaksi Id {TransactionId} berhasil dibuat dan di-commit untuk UserId: {UserId}",
                transaction.Id, userId);

            return MapToResponse(transaction);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Rollback transaksi karena terjadi error saat membuat transaksi untuk UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<TransactionResponse?> UpdateAsync(
        int id,
        UpdateTransactionRequest request,
        int userId)
    {
        _logger.LogInformation("Memulai pembaruan transaksi Id {TransactionId} untuk UserId: {UserId}", id, userId);

        var transaction =
            await _transactionRepository.GetByIdAsync(id, userId);

        if (transaction == null)
        {
            _logger.LogWarning("Gagal memperbarui: Transaksi Id {TransactionId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        var oldWallet = await _walletRepository.GetByIdAsync(
            transaction.WalletId,
            userId);

        if (oldWallet == null)
        {
            _logger.LogWarning("Gagal memperbarui: Dompet lama Id {WalletId} tidak ditemukan", transaction.WalletId);
            return null;
        }

        var newWallet = await _walletRepository.GetByIdAsync(
            request.WalletId,
            userId);

        if (newWallet == null)
        {
            _logger.LogWarning("Gagal memperbarui: Dompet baru Id {WalletId} tidak ditemukan", request.WalletId);
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            _logger.LogWarning("Gagal memperbarui: Kategori Id {CategoryId} tidak ditemukan", request.CategoryId);
            return null;
        }

        if (request.Amount <= 0 || (request.Type != "Income" && request.Type != "Expense"))
        {
            _logger.LogWarning("Gagal memperbarui: Data transaksi tidak valid (Amount: {Amount}, Type: {Type})",
                request.Amount, request.Type);
            return null;
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Kembalikan efek transaksi lama
            if (transaction.Type == "Income")
            {
                oldWallet.Balance -= transaction.Amount;
            }
            else
            {
                oldWallet.Balance += transaction.Amount;
            }

            // 2. Terapkan efek transaksi baru
            if (oldWallet.Id == newWallet.Id)
            {
                if (request.Type == "Income")
                {
                    oldWallet.Balance += request.Amount;
                }
                else
                {
                    oldWallet.Balance -= request.Amount;
                }

                await _walletRepository.UpdateAsync(oldWallet);
            }
            else
            {
                if (request.Type == "Income")
                {
                    newWallet.Balance += request.Amount;
                }
                else
                {
                    newWallet.Balance -= request.Amount;
                }

                await _walletRepository.UpdateAsync(oldWallet);
                await _walletRepository.UpdateAsync(newWallet);
            }

            transaction.WalletId = request.WalletId;
            transaction.CategoryId = request.CategoryId;
            transaction.Amount = request.Amount;
            transaction.Type = request.Type;
            transaction.Description = request.Description?.Trim();
            transaction.TransactionDate = request.TransactionDate;

            await _transactionRepository.UpdateAsync(transaction);

            await dbTransaction.CommitAsync();

            _logger.LogInformation("Transaksi Id {TransactionId} berhasil diperbarui dan di-commit untuk UserId: {UserId}",
                transaction.Id, userId);

            return MapToResponse(transaction);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Rollback transaksi karena terjadi error saat memperbarui transaksi Id {TransactionId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Memulai penghapusan transaksi Id {TransactionId} untuk UserId: {UserId}", id, userId);

        var transaction =
            await _transactionRepository.GetByIdAsync(id, userId);

        if (transaction == null)
        {
            _logger.LogWarning("Gagal menghapus: Transaksi Id {TransactionId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return false;
        }

        var wallet = await _walletRepository.GetByIdAsync(
            transaction.WalletId,
            userId);

        if (wallet == null)
        {
            _logger.LogWarning("Gagal menghapus: Dompet terkait Id {WalletId} tidak ditemukan", transaction.WalletId);
            return false;
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Kembalikan saldo seperti sebelum transaksi dibuat
            if (transaction.Type == "Income")
            {
                wallet.Balance -= transaction.Amount;
            }
            else
            {
                wallet.Balance += transaction.Amount;
            }

            await _transactionRepository.DeleteAsync(transaction);
            await _walletRepository.UpdateAsync(wallet);

            await dbTransaction.CommitAsync();

            _logger.LogInformation("Transaksi Id {TransactionId} berhasil dihapus dan saldo dompet Id {WalletId} dikembalikan",
                id, wallet.Id);

            return true;
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Rollback karena terjadi error saat menghapus transaksi Id {TransactionId}", id);
            throw;
        }
    }

    private static TransactionResponse MapToResponse(
        Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            CategoryId = transaction.CategoryId,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        };
    }
}