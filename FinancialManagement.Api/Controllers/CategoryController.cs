using System.Security.Claims;
using FinancialManagement.Api.DTOs.Category;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var categories = await _categoryService.GetAllAsync(userId);

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var category = await _categoryService.GetByIdAsync(
            id,
            userId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category tidak ditemukan."
            });
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCategoryRequest request)
    {
        var userId = GetUserId();

        var category = await _categoryService.CreateAsync(
            request,
            userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCategoryRequest request)
    {
        var userId = GetUserId();

        var category = await _categoryService.UpdateAsync(
            id,
            request,
            userId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category tidak ditemukan."
            });
        }

        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted = await _categoryService.DeleteAsync(
            id,
            userId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Category tidak ditemukan."
            });
        }

        return Ok(new
        {
            message = "Category berhasil dihapus."
        });
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}