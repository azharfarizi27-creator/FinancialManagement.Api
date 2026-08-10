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

    public TransactionService(
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<List<TransactionResponse>> GetAllAsync(int userId)
    {
        var transactions =
            await _transactionRepository.GetByUserIdAsync(userId);

        return transactions
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<TransactionResponse?> GetByIdAsync(
        int id,
        int userId)
    {
        var transaction =
            await _transactionRepository.GetByIdAsync(id, userId);

        if (transaction == null)
        {
            return null;
        }

        return MapToResponse(transaction);
    }

    public async Task<TransactionResponse?> CreateAsync(
        CreateTransactionRequest request,
        int userId)
    {
        var wallet = await _walletRepository.GetByIdAsync(
            request.WalletId,
            userId);

        if (wallet == null)
        {
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            return null;
        }

        if (request.Amount <= 0)
        {
            return null;
        }

        if (request.Type != "Income" &&
            request.Type != "Expense")
        {
            return null;
        }

        var transaction = new Transaction
        {
            UserId = userId,
            WalletId = request.WalletId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Type = request.Type,
            Description = request.Description,
            TransactionDate = request.TransactionDate,
            CreatedAt = DateTime.UtcNow
        };

        // Update saldo Wallet
        if (request.Type == "Income")
        {
            wallet.Balance += request.Amount;
        }
        else
        {
            wallet.Balance -= request.Amount;
        }

        await _transactionRepository.CreateAsync(transaction);

        await _walletRepository.UpdateAsync(wallet);

        return MapToResponse(transaction);
    }

    public async Task<TransactionResponse?> UpdateAsync(
        int id,
        UpdateTransactionRequest request,
        int userId)
    {
        var transaction =
            await _transactionRepository.GetByIdAsync(id, userId);

        if (transaction == null)
        {
            return null;
        }

        var oldWallet = await _walletRepository.GetByIdAsync(
            transaction.WalletId,
            userId);

        if (oldWallet == null)
        {
            return null;
        }

        var newWallet = await _walletRepository.GetByIdAsync(
            request.WalletId,
            userId);

        if (newWallet == null)
        {
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId);

        if (category == null)
        {
            return null;
        }

        if (request.Amount <= 0)
        {
            return null;
        }

        if (request.Type != "Income" &&
            request.Type != "Expense")
        {
            return null;
        }

        // Kembalikan efek transaksi lama
        if (transaction.Type == "Income")
        {
            oldWallet.Balance -= transaction.Amount;
        }
        else
        {
            oldWallet.Balance += transaction.Amount;
        }

        // Terapkan efek transaksi baru
        if (request.Type == "Income")
        {
            newWallet.Balance += request.Amount;
        }
        else
        {
            newWallet.Balance -= request.Amount;
        }

        transaction.WalletId = request.WalletId;
        transaction.CategoryId = request.CategoryId;
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Description = request.Description;
        transaction.TransactionDate = request.TransactionDate;

        await _transactionRepository.UpdateAsync(transaction);

        // Jika Wallet lama dan baru berbeda,
        // keduanya perlu disimpan.
        if (oldWallet.Id == newWallet.Id)
        {
            await _walletRepository.UpdateAsync(oldWallet);
        }
        else
        {
            await _walletRepository.UpdateAsync(oldWallet);
            await _walletRepository.UpdateAsync(newWallet);
        }

        return MapToResponse(transaction);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var transaction =
            await _transactionRepository.GetByIdAsync(id, userId);

        if (transaction == null)
        {
            return false;
        }

        var wallet = await _walletRepository.GetByIdAsync(
            transaction.WalletId,
            userId);

        if (wallet == null)
        {
            return false;
        }

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

        return true;
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