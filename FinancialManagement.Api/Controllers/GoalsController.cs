using FinancialManagement.Api.DTOs.Goal;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class GoalsController : BaseApiController
{
    private readonly IGoalService _goalService;

    public GoalsController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var goals = await _goalService.GetAllAsync(userId);
        return Ok(goals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var goal = await _goalService.GetByIdAsync(id, userId);
        if (goal == null)
        {
            return NotFound(new
            {
                message = "Target tabungan tidak ditemukan."
            });
        }
        return Ok(goal);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGoalRequest request)
    {
        var userId = GetUserId();
        var goal = await _goalService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateGoalRequest request)
    {
        var userId = GetUserId();
        var goal = await _goalService.UpdateAsync(id, request, userId);
        if (goal == null)
        {
            return NotFound(new
            {
                message = "Target tabungan tidak ditemukan."
            });
        }
        return Ok(goal);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var deleted = await _goalService.DeleteAsync(id, userId);
        if (!deleted)
        {
            return NotFound(new
            {
                message = "Target tabungan tidak ditemukan."
            });
        }
        return Ok(new
        {
            message = "Target tabungan berhasil dihapus."
        });
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(int id, DepositGoalRequest request)
    {
        var userId = GetUserId();
        var updatedGoal = await _goalService.DepositAsync(id, request, userId);
        return Ok(updatedGoal);
    }
}
