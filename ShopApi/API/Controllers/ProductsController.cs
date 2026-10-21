
using System.Security.Claims;
using Application.DTOs.Products;
using Application.Interfaces;
using Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ProductFilter filter,
        CancellationToken ct)
    {
        var result = await productService.GetPagedAsync(filter, ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await productService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProductDto dto,
        CancellationToken ct)
    {
        int userId = GetCurrentUserId();
        var result = await productService.CreateAsync(dto, userId, ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductDto dto,
        CancellationToken ct)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = User.IsInRole("Admin");
        var result = await productService.UpdateAsync(
            id, dto, userId, isAdmin, ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = User.IsInRole("Admin");
        var result = await productService.DeleteAsync(
            id, userId, isAdmin, ct);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out int id))
            throw new UnauthorizedAccessException("Invalid user ID in token.");
        return id;
    }
}
