using FinancialManagement.Api.DTOs.Bill;

namespace FinancialManagement.Api.Services.Interfaces;

public interface IBillService
{
    Task<List<BillResponse>> GetAllAsync(int userId);

    Task<BillResponse?> GetByIdAsync(int id, int userId);

    Task<BillResponse> CreateAsync(CreateBillRequest request, int userId);

    Task<BillResponse?> UpdateAsync(int id, UpdateBillRequest request, int userId);

    Task<bool> DeleteAsync(int id, int userId);

    Task<BillResponse> PayAsync(int id, PayBillRequest request, int userId);

    Task<BillResponse> ToggleStatusAsync(int id, int userId);
}
