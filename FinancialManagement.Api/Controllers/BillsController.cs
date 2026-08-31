using FinancialManagement.Api.DTOs.Bill;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class BillsController : BaseApiController
{
    private readonly IBillService _billService;

    public BillsController(IBillService billService)
    {
        _billService = billService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var bills = await _billService.GetAllAsync(userId);
        return Ok(bills);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var bill = await _billService.GetByIdAsync(id, userId);
        if (bill == null)
        {
            return NotFound(new
            {
                message = "Tagihan tidak ditemukan."
            });
        }
        return Ok(bill);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBillRequest request)
    {
        var userId = GetUserId();
        var bill = await _billService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = bill.Id }, bill);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateBillRequest request)
    {
        var userId = GetUserId();
        var bill = await _billService.UpdateAsync(id, request, userId);
        if (bill == null)
        {
            return NotFound(new
            {
                message = "Tagihan tidak ditemukan."
            });
        }
        return Ok(bill);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var deleted = await _billService.DeleteAsync(id, userId);
        if (!deleted)
        {
            return NotFound(new
            {
                message = "Tagihan tidak ditemukan."
            });
        }
        return Ok(new
        {
            message = "Tagihan berhasil dihapus."
        });
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(int id, PayBillRequest request)
    {
        var userId = GetUserId();
        var updatedBill = await _billService.PayAsync(id, request, userId);
        return Ok(updatedBill);
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var userId = GetUserId();
        var updatedBill = await _billService.ToggleStatusAsync(id, userId);
        return Ok(updatedBill);
    }
}
