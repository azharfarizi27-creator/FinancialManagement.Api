using System.Security.Claims;
using FinancialManagement.Api.DTOs.Budget;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetController(
        IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var budgets =
            await _budgetService.GetAllAsync(userId);

        return Ok(budgets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var budget =
            await _budgetService.GetByIdAsync(
                id,
                userId);

        if (budget == null)
        {
            return NotFound(new
            {
                message = "Budget tidak ditemukan."
            });
        }

        return Ok(budget);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateBudgetRequest request)
    {
        var userId = GetUserId();

        if (request.LimitAmount <= 0)
        {
            return BadRequest(new
            {
                message = "LimitAmount harus lebih besar dari 0."
            });
        }

        if (request.Month < 1 || request.Month > 12)
        {
            return BadRequest(new
            {
                message = "Month harus berada di antara 1 dan 12."
            });
        }

        if (request.Year < 2000 || request.Year > 2100)
        {
            return BadRequest(new
            {
                message = "Year harus berada di antara 2000 dan 2100."
            });
        }

        var budget =
            await _budgetService.CreateAsync(
                request,
                userId);

        if (budget == null)
        {
            return BadRequest(new
            {
                message =
                    "Category tidak ditemukan atau budget untuk periode tersebut sudah ada."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = budget.Id },
            budget);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateBudgetRequest request)
    {
        var userId = GetUserId();

        if (request.LimitAmount <= 0)
        {
            return BadRequest(new
            {
                message = "LimitAmount harus lebih besar dari 0."
            });
        }

        if (request.Month < 1 || request.Month > 12)
        {
            return BadRequest(new
            {
                message = "Month harus berada di antara 1 dan 12."
            });
        }

        if (request.Year < 2000 || request.Year > 2100)
        {
            return BadRequest(new
            {
                message = "Year harus berada di antara 2000 dan 2100."
            });
        }

        var budget =
            await _budgetService.UpdateAsync(
                id,
                request,
                userId);

        if (budget == null)
        {
            return BadRequest(new
            {
                message =
                    "Budget, Category, atau periode budget tidak valid."
            });
        }

        return Ok(budget);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted =
            await _budgetService.DeleteAsync(
                id,
                userId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Budget tidak ditemukan."
            });
        }

        return Ok(new
        {
            message = "Budget berhasil dihapus."
        });
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}