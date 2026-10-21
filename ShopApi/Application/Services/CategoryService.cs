using Application.ApiResult;
using Application.DTOs.Categories;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class CategoryService(ICategoryRepository repository)
    : ICategoryService
{
    public async Task<Result<List<CategoryResponseDto>>> GetAllAsync(
        CancellationToken ct)
    {
        var categories = await repository.GetAllAsync(ct);

        var result = categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        return Result<List<CategoryResponseDto>>.Success(result);
    }

    public async Task<Result<CategoryResponseDto>> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(id, ct);

        if (category is null)
        {
            return Result<CategoryResponseDto>.Fail(
                "Category not found.",
                ErrorType.NotFound);
        }

        var result = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return Result<CategoryResponseDto>.Success(result);
    }

    public async Task<Result<CategoryResponseDto>> CreateAsync(
        CreateCategoryDto dto,
        CancellationToken ct)
    {
        var name = dto.Name.Trim();

        if (name.Length < 2 || name.Length > 100)
        {
            return Result<CategoryResponseDto>.Fail(
                "Category name must contain 2-100 characters.",
                ErrorType.Validation);
        }

        var existing = await repository.GetByNameAsync(name, ct);

        if (existing is not null)
        {
            return Result<CategoryResponseDto>.Fail(
                "Category with this name already exists.",
                ErrorType.Conflict);
        }

        var category = new Category
        {
            Name = name,
            Description = dto.Description?.Trim()
        };

        await repository.CreateAsync(category);

        var result = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return Result<CategoryResponseDto>.Success(result);
    }

    public async Task<Result<CategoryResponseDto>> UpdateAsync(
        int id,
        UpdateCategoryDto dto,
        CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(id, ct);

        if (category is null)
        {
            return Result<CategoryResponseDto>.Fail(
                "Category not found.",
                ErrorType.NotFound);
        }

        var name = dto.Name.Trim();

        if (name.Length < 2 || name.Length > 100)
        {
            return Result<CategoryResponseDto>.Fail(
                "Category name must contain 2-100 characters.",
                ErrorType.Validation);
        }

        var existing = await repository.GetByNameAsync(name, ct);

        if (existing is not null && existing.Id != id)
        {
            return Result<CategoryResponseDto>.Fail(
                "Category with this name already exists.",
                ErrorType.Conflict);
        }

        category.Name = name;
        category.Description = dto.Description?.Trim();

        await repository.UpdateAsync(category);

        var result = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return Result<CategoryResponseDto>.Success(result);
    }

    public async Task<Result<bool>> DeleteAsync(
        int id,
        CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(id, ct);

        if (category is null)
        {
            return Result<bool>.Fail(
                "Category not found.",
                ErrorType.NotFound);
        }

        var hasProducts = await repository.HasProductsAsync(id, ct);

        if (hasProducts)
        {
            return Result<bool>.Fail(
                "Cannot delete a category that contains products.",
                ErrorType.Conflict);
        }

        await repository.DeleteAsync(category);

        return Result<bool>.Success(true);
    }
}