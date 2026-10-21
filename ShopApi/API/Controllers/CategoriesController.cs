using Application.DTOs.Categories;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await categoryService.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await categoryService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCategoryDto dto,
        CancellationToken ct)
    {
        var result = await categoryService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCategoryDto dto,
        CancellationToken ct)
    {
        var result = await categoryService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await categoryService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return NoContent();
    }
}
