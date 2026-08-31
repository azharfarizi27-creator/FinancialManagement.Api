using FinancialManagement.Api.Data;
using FinancialManagement.Api.DTOs.Bill;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Services.Impl;

public class BillService : IBillService
{
    private readonly IBillRepository _billRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<BillService> _logger;

    public BillService(
        IBillRepository billRepository,
        IWalletRepository walletRepository,
        AppDbContext context,
        ILogger<BillService> logger)
    {
        _billRepository = billRepository;
        _walletRepository = walletRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<List<BillResponse>> GetAllAsync(int userId)
    {
        _logger.LogInformation("Mengambil semua tagihan berulang untuk UserId {UserId}", userId);

        var bills = await _billRepository.GetByUserIdAsync(userId);

        return bills.Select(MapToResponse).ToList();
    }

    public async Task<BillResponse?> GetByIdAsync(int id, int userId)
    {
        _logger.LogInformation("Mengambil tagihan berulang Id {BillId} untuk UserId {UserId}", id, userId);

        var bill = await _billRepository.GetByIdAsync(id, userId);
        if (bill == null)
        {
            return null;
        }

        return MapToResponse(bill);
    }

    public async Task<BillResponse> CreateAsync(CreateBillRequest request, int userId)
    {
        var title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title.Trim() : (request.Name?.Trim() ?? string.Empty);

        _logger.LogInformation("Membuat tagihan berulang baru '{Title}' (Amount: {Amount}, Freq: {Freq}) untuk UserId {UserId}",
            title, request.Amount, request.Frequency, userId);

        var bill = new RecurringBill
        {
            UserId = userId,
            Title = title,
            Amount = request.Amount,
            DueDate = request.DueDate,
            Frequency = NormalizeFrequency(request.Frequency),
            CategoryId = request.CategoryId,
            ReminderDays = request.ReminderDays,
            IsPaidThisCycle = false,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _billRepository.CreateAsync(bill);

        _logger.LogInformation("Tagihan berulang Id {BillId} berhasil dibuat untuk UserId {UserId}", bill.Id, userId);

        var createdBill = await _billRepository.GetByIdAsync(bill.Id, userId);
        return MapToResponse(createdBill ?? bill);
    }

    public async Task<BillResponse?> UpdateAsync(int id, UpdateBillRequest request, int userId)
    {
        _logger.LogInformation("Memperbarui tagihan berulang Id {BillId} untuk UserId {UserId}", id, userId);

        var bill = await _billRepository.GetTrackedByIdAsync(id, userId);
        if (bill == null)
        {
            return null;
        }

        var title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title.Trim() : (request.Name?.Trim() ?? bill.Title);

        bill.Title = title;
        bill.Amount = request.Amount;
        bill.DueDate = request.DueDate;
        bill.Frequency = NormalizeFrequency(request.Frequency);
        bill.CategoryId = request.CategoryId;
        bill.ReminderDays = request.ReminderDays;

        var existingPaid = (bill.Payments ?? Enumerable.Empty<RecurringBillPayment>()).Sum(p => p.Amount);
        if (request.IsPaidThisCycle.HasValue)
        {
            bill.IsPaidThisCycle = request.IsPaidThisCycle.Value;
        }
        else if (existingPaid < bill.Amount)
        {
            // Jika nominal tagihan dinaikkan dan total bayar belum cukup, jangan tandai lunas
            bill.IsPaidThisCycle = false;
        }
        bill.Notes = request.Notes?.Trim();

        await _billRepository.UpdateAsync(bill);

        _logger.LogInformation("Tagihan berulang Id {BillId} berhasil diperbarui", id);

        var updatedBill = await _billRepository.GetByIdAsync(id, userId);
        return MapToResponse(updatedBill ?? bill);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        _logger.LogInformation("Menghapus tagihan berulang Id {BillId} untuk UserId {UserId}", id, userId);

        var bill = await _billRepository.GetTrackedByIdAsync(id, userId);
        if (bill == null)
        {
            return false;
        }

        await _billRepository.DeleteAsync(bill);

        _logger.LogInformation("Tagihan berulang Id {BillId} berhasil dihapus", id);

        return true;
    }

    public async Task<BillResponse> PayAsync(int id, PayBillRequest request, int userId)
    {
        _logger.LogInformation("Memproses pembayaran tagihan Id {BillId} untuk UserId {UserId}", id, userId);

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var bill = await _billRepository.GetTrackedByIdAsync(id, userId);
            if (bill == null)
            {
                throw new NotFoundException("Tagihan tidak ditemukan.");
            }

            var payAmount = request.Amount ?? bill.Amount;
            if (payAmount <= 0)
            {
                throw new BadRequestException("Nominal pembayaran tagihan tidak valid.");
            }

            var payDate = request.PaymentDate ?? DateTime.UtcNow;

            if (request.WalletId.HasValue)
            {
                var wallet = await _walletRepository.GetTrackedByIdAsync(request.WalletId.Value, userId);
                if (wallet == null)
                {
                    throw new NotFoundException("Dompet untuk pembayaran tidak ditemukan.");
                }

                if (wallet.Balance < payAmount)
                {
                    throw new BadRequestException($"Saldo dompet '{wallet.Name}' (Rp {wallet.Balance:N0}) tidak mencukupi untuk membayar tagihan Rp {payAmount:N0}.");
                }

                wallet.Balance -= payAmount;

                var categoryId = bill.CategoryId;
                if (!categoryId.HasValue)
                {
                    var cat = await _context.Categories
                        .FirstOrDefaultAsync(c => c.UserId == userId && (c.Name == "Tagihan" || c.Name == "Utilitas" || c.Name == "Lainnya"));
                    if (cat == null)
                    {
                        cat = await _context.Categories.FirstOrDefaultAsync(c => c.UserId == userId);
                    }
                    categoryId = cat?.Id;
                }

                var noteText = !string.IsNullOrWhiteSpace(request.Note) ? $" ({request.Note.Trim()})" : "";
                var tx = new Transaction
                {
                    UserId = userId,
                    WalletId = wallet.Id,
                    CategoryId = categoryId ?? 1,
                    Amount = payAmount,
                    Type = "Expense",
                    Description = $"Bayar Tagihan: {bill.Title}{noteText}",
                    TransactionDate = payDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(tx);
            }

            var currentPaid = (bill.Payments ?? Enumerable.Empty<RecurringBillPayment>()).Sum(p => p.Amount);
            var totalPaidAfterThis = currentPaid + payAmount;
            var isFullPayment = totalPaidAfterThis >= bill.Amount;

            var payment = new RecurringBillPayment
            {
                RecurringBillId = bill.Id,
                WalletId = request.WalletId,
                Amount = payAmount,
                PaidDate = payDate,
                Note = !string.IsNullOrWhiteSpace(request.Note) 
                    ? request.Note.Trim() 
                    : (isFullPayment ? $"Pelunasan tagihan periode {payDate:yyyy-MM-dd}" : $"Pembayaran sebagian tagihan (Rp {payAmount:N0} / Rp {bill.Amount:N0})")
            };

            _context.RecurringBillPayments.Add(payment);

            bill.LastPaidDate = payDate;
            if (isFullPayment)
            {
                bill.IsPaidThisCycle = true;
                bill.DueDate = AdvanceDueDate(bill.DueDate, bill.Frequency);
            }
            else
            {
                bill.IsPaidThisCycle = false;
            }

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            _logger.LogInformation("Pembayaran tagihan Id {BillId} berhasil diproses. DueDate selanjutnya: {NextDueDate}",
                bill.Id, bill.DueDate);

            var updatedBill = await _billRepository.GetByIdAsync(bill.Id, userId);
            return MapToResponse(updatedBill ?? bill);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Gagal memproses pembayaran tagihan Id {BillId} untuk UserId {UserId}", id, userId);
            throw;
        }
    }

    public async Task<BillResponse> ToggleStatusAsync(int id, int userId)
    {
        _logger.LogInformation("Mengubah status pembayaran tagihan Id {BillId} untuk UserId {UserId}", id, userId);

        var bill = await _billRepository.GetTrackedByIdAsync(id, userId);
        if (bill == null)
        {
            throw new NotFoundException("Tagihan tidak ditemukan.");
        }

        bill.IsPaidThisCycle = !bill.IsPaidThisCycle;
        if (bill.IsPaidThisCycle)
        {
            bill.LastPaidDate = DateTime.UtcNow;
        }

        await _billRepository.UpdateAsync(bill);

        var updatedBill = await _billRepository.GetByIdAsync(id, userId);
        return MapToResponse(updatedBill ?? bill);
    }

    private static DateTime AdvanceDueDate(DateTime current, string frequency)
    {
        return frequency.ToLowerInvariant() switch
        {
            "weekly" => current.AddDays(7),
            "yearly" => current.AddYears(1),
            _ => current.AddMonths(1)
        };
    }

    private static string NormalizeFrequency(string? frequency)
    {
        if (string.IsNullOrWhiteSpace(frequency)) return "monthly";
        var freq = frequency.Trim().ToLowerInvariant();
        return freq is "weekly" or "monthly" or "yearly" ? freq : "monthly";
    }

    private static BillResponse MapToResponse(RecurringBill bill)
    {
        var payments = (bill.Payments ?? Enumerable.Empty<RecurringBillPayment>())
            .OrderByDescending(p => p.PaidDate)
            .Select(p => new BillPaymentResponse
            {
                Id = p.Id,
                RecurringBillId = p.RecurringBillId,
                WalletId = p.WalletId,
                WalletName = p.Wallet?.Name,
                Amount = p.Amount,
                Date = p.PaidDate,
                Note = p.Note
            })
            .ToList();

        var totalPaid = payments.Sum(p => p.Amount);

        return new BillResponse
        {
            Id = bill.Id,
            Title = bill.Title,
            Amount = bill.Amount,
            PaidAmount = bill.IsPaidThisCycle ? bill.Amount : totalPaid,
            DueDate = bill.DueDate,
            Frequency = bill.Frequency,
            CategoryId = bill.CategoryId,
            CategoryName = bill.Category?.Name,
            ReminderDays = bill.ReminderDays,
            IsPaidThisCycle = bill.IsPaidThisCycle,
            LastPaidDate = bill.LastPaidDate,
            Notes = bill.Notes,
            CreatedAt = bill.CreatedAt,
            Payments = payments,
            History = payments
        };
    }
}
