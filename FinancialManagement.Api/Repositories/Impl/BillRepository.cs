using FinancialManagement.Api.Data;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagement.Api.Repositories.Impl;

public class BillRepository : IBillRepository
{
    private readonly AppDbContext _context;

    public BillRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecurringBill>> GetByUserIdAsync(int userId)
    {
        return await _context.RecurringBills
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Payments)
                .ThenInclude(p => p.Wallet)
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    public async Task<RecurringBill?> GetByIdAsync(int id, int userId)
    {
        return await _context.RecurringBills
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Payments)
                .ThenInclude(p => p.Wallet)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<RecurringBill?> GetTrackedByIdAsync(int id, int userId)
    {
        return await _context.RecurringBills
            .Include(b => b.Category)
            .Include(b => b.Payments)
                .ThenInclude(p => p.Wallet)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<RecurringBill> CreateAsync(RecurringBill bill)
    {
        _context.RecurringBills.Add(bill);
        await _context.SaveChangesAsync();
        return bill;
    }

    public async Task UpdateAsync(RecurringBill bill)
    {
        _context.RecurringBills.Update(bill);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RecurringBill bill)
    {
        _context.RecurringBills.Remove(bill);
        await _context.SaveChangesAsync();
    }

    public async Task<RecurringBillPayment> AddPaymentAsync(RecurringBillPayment payment)
    {
        _context.RecurringBillPayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
}
