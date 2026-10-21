using Application.ApiResult;
using Application.DTOs.Categories;

namespace Application.Interfaces;

public interface ICategoryService
{
    Task<Result<List<CategoryResponseDto>>> GetAllAsync(
        CancellationToken ct);

    Task<Result<CategoryResponseDto>> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<Result<CategoryResponseDto>> CreateAsync(
        CreateCategoryDto dto,
        CancellationToken ct);

    Task<Result<CategoryResponseDto>> UpdateAsync(
        int id,
        UpdateCategoryDto dto,
        CancellationToken ct);

    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);
}