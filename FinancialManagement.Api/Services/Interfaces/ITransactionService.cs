using FinancialManagement.Api.DTOs.Transaction;
using FinancialManagement.Api.DTOs.Common;

namespace FinancialManagement.Api.Services.Interfaces;

public interface ITransactionService
{
    Task<PagedResponse<TransactionResponse>> GetAllAsync(
        int userId,
        int page = 1,
        int pageSize = 10,
        string? type = null,
        int? categoryId = null,
        int? walletId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

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

    Task<TransactionResponse?> UploadReceiptAsync(
        int id,
        int userId,
        IFormFile file);

    Task<bool> DeleteAsync(
        int id,
        int userId);
}