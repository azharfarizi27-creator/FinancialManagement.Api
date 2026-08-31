using FinancialManagement.Api.Models;

namespace FinancialManagement.Api.Repositories.Interfaces;

public interface IBillRepository
{
    Task<List<RecurringBill>> GetByUserIdAsync(int userId);

    Task<RecurringBill?> GetByIdAsync(int id, int userId);

    Task<RecurringBill?> GetTrackedByIdAsync(int id, int userId);

    Task<RecurringBill> CreateAsync(RecurringBill bill);

    Task UpdateAsync(RecurringBill bill);

    Task DeleteAsync(RecurringBill bill);

    Task<RecurringBillPayment> AddPaymentAsync(RecurringBillPayment payment);
}
