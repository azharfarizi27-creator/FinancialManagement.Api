using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.Wallet;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Services.Impl;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _repository;
    private readonly AppDbContext _context;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository repository,
        AppDbContext context,
        ILogger<WalletService> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    public async Task<List<WalletResponse>> GetAllAsync(int userId)
    {
        _logger.LogInformation("Mengambil seluruh dompet untuk UserId: {UserId}", userId);

        var wallets = await _repository.GetByUserIdAsync(userId);

        _logger.LogInformation("Ditemukan {Count} dompet untuk UserId: {UserId}", wallets.Count, userId);

        return wallets
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<WalletResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mengambil dompet Id {WalletId} untuk UserId: {UserId}", id, userId);

        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            _logger.LogWarning("Dompet Id {WalletId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse> CreateAsync(
        CreateWalletRequest request,
        int userId)
    {
        _logger.LogInformation("Membuat dompet baru '{Name}' (Type: {Type}, Balance: {Balance}) untuk UserId: {UserId}",
            request.Name, request.Type, request.Balance, userId);

        var wallet = new Wallet
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Balance = request.Balance,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(wallet);

        _logger.LogInformation("Dompet berhasil dibuat dengan Id {WalletId} untuk UserId: {UserId}", wallet.Id, userId);

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse?> UpdateAsync(
        int id,
        UpdateWalletRequest request,
        int userId)
    {
        _logger.LogInformation("Memperbarui dompet Id {WalletId} untuk UserId: {UserId}", id, userId);

        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            _logger.LogWarning("Gagal memperbarui: Dompet Id {WalletId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return null;
        }

        wallet.Name = request.Name.Trim();
        wallet.Type = request.Type.Trim();
        wallet.Balance = request.Balance;

        await _repository.UpdateAsync(wallet);

        _logger.LogInformation("Dompet Id {WalletId} berhasil diperbarui untuk UserId: {UserId}", id, userId);

        return MapToResponse(wallet);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        _logger.LogInformation("Mencoba menghapus dompet Id {WalletId} untuk UserId: {UserId}", id, userId);

        var wallet = await _repository.GetByIdAsync(id, userId);

        if (wallet == null)
        {
            _logger.LogWarning("Gagal menghapus: Dompet Id {WalletId} tidak ditemukan untuk UserId: {UserId}", id, userId);
            return false;
        }

        await _repository.DeleteAsync(wallet);

        _logger.LogInformation("Dompet Id {WalletId} berhasil dihapus untuk UserId: {UserId}", id, userId);

        return true;
    }

    public async Task<TransferResponse> TransferAsync(
        TransferWalletRequest request,
        int userId)
    {
        _logger.LogInformation("Memulai transfer saldo dari WalletId {FromId} ke WalletId {ToId} untuk UserId {UserId}",
            request.FromWalletId, request.ToWalletId, userId);

        if (request.FromWalletId == request.ToWalletId)
        {
            throw new BadRequestException("Dompet asal dan tujuan tidak boleh sama.");
        }

        if (request.Amount <= 0)
        {
            throw new BadRequestException("Nominal transfer harus lebih besar dari 0.");
        }

        if (request.AdminFee < 0)
        {
            throw new BadRequestException("Biaya admin tidak boleh negatif.");
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var fromWallet = await _repository.GetTrackedByIdAsync(request.FromWalletId, userId);
            var toWallet = await _repository.GetTrackedByIdAsync(request.ToWalletId, userId);

            if (fromWallet == null || toWallet == null)
            {
                throw new NotFoundException("Dompet asal atau tujuan tidak ditemukan atau bukan milik Anda.");
            }

            var totalDebit = request.Amount + request.AdminFee;

            if (fromWallet.Balance < totalDebit)
            {
                throw new BadRequestException($"Saldo dompet asal ({fromWallet.Name}: Rp {fromWallet.Balance:N0}) tidak mencukupi untuk transfer Rp {request.Amount:N0} + biaya admin Rp {request.AdminFee:N0}.");
            }

            fromWallet.Balance -= totalDebit;
            toWallet.Balance += request.Amount;

            var transferDate = request.Date ?? DateTime.UtcNow;

            // Cari atau buat kategori Transfer untuk user
            var transferCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && (c.Name == "Transfer" || c.Name == "Lainnya"));

            if (transferCategory == null)
            {
                transferCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (transferCategory == null)
                {
                    transferCategory = new Category
                    {
                        UserId = userId,
                        Name = "Transfer Saldo",
                        Type = "Expense"
                    };
                    _context.Categories.Add(transferCategory);
                    await _context.SaveChangesAsync();
                }
            }

            // Catat log transaksi debet dari dompet asal
            var outgoingTx = new Transaction
            {
                UserId = userId,
                WalletId = fromWallet.Id,
                CategoryId = transferCategory.Id,
                Amount = totalDebit,
                Type = "Expense",
                Description = $"Transfer keluar ke {toWallet.Name}" + (!string.IsNullOrWhiteSpace(request.Notes) ? $" ({request.Notes.Trim()})" : ""),
                TransactionDate = transferDate,
                CreatedAt = DateTime.UtcNow
            };

            // Catat log transaksi kredit ke dompet tujuan
            var incomingTx = new Transaction
            {
                UserId = userId,
                WalletId = toWallet.Id,
                CategoryId = transferCategory.Id,
                Amount = request.Amount,
                Type = "Income",
                Description = $"Transfer masuk dari {fromWallet.Name}" + (!string.IsNullOrWhiteSpace(request.Notes) ? $" ({request.Notes.Trim()})" : ""),
                TransactionDate = transferDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(outgoingTx);
            _context.Transactions.Add(incomingTx);

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            _logger.LogInformation("Transfer berhasil: Rp {Amount} dari Wallet {FromId} ke Wallet {ToId} untuk UserId {UserId}",
                request.Amount, fromWallet.Id, toWallet.Id, userId);

            return new TransferResponse
            {
                Success = true,
                Message = $"Transfer sebesar Rp {request.Amount:N0} dari {fromWallet.Name} ke {toWallet.Name} berhasil diproses.",
                TransferredAmount = request.Amount,
                AdminFee = request.AdminFee,
                FromWalletNewBalance = fromWallet.Balance,
                ToWalletNewBalance = toWallet.Balance,
                TransactionDate = transferDate
            };
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Rollback transfer dana untuk UserId: {UserId}", userId);
            throw;
        }
    }

    private static WalletResponse MapToResponse(Wallet wallet)
    {
        return new WalletResponse
        {
            Id = wallet.Id,
            Name = wallet.Name,
            Type = wallet.Type,
            Balance = wallet.Balance,
            CreatedAt = wallet.CreatedAt
        };
    }
}