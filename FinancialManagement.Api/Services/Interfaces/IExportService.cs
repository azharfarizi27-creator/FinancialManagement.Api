namespace FinancialManagement.Api.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportTransactionsCsvAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        int? categoryId,
        int? walletId);

    Task<byte[]> ExportTransactionsExcelAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        int? categoryId,
        int? walletId);

    Task<byte[]> ExportTransactionsPdfAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        int? categoryId,
        int? walletId);

    Task<byte[]> ExportFinancialReportPdfAsync(
        int userId,
        int? month,
        int? year,
        int? walletId,
        DateTime? startDate,
        DateTime? endDate);
}
