using FinancialManagement.Api.DTOs.Transaction;

namespace FinancialManagement.Api.Services.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionResponse>> GetAllAsync(int userId);

    Task<TransactionResponse?> GetByIdAsync(
        int id,
        int userId);

    Task<TransactionResponse?> CreateAsync(
        CreateTransactionRequest request,
        int userId);

    Task<TransactionResponse?> UpdateAsync(
        int id,
        UpdateTransactionRequest request,
        int userId);

    Task<bool> DeleteAsync(
        int id,
        int userId);
}