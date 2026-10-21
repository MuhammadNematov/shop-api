using Application.ApiResult;
using Application.DTOs.Products;
using Domain.Filters;

namespace Application.Interfaces;

public interface IProductService
{
    Task<Result<PagedResult<ProductResponseDto>>> GetPagedAsync(
        ProductFilter filter,
        CancellationToken ct);

    Task<Result<ProductResponseDto>> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<Result<ProductResponseDto>> CreateAsync(
        CreateProductDto dto,
        int currentUserId,
        CancellationToken ct);

    Task<Result<ProductResponseDto>> UpdateAsync(
        int id,
        UpdateProductDto dto,
        int currentUserId,
        bool isAdmin,
        CancellationToken ct);

    Task<Result<bool>> DeleteAsync(
        int id,
        int currentUserId,
        bool isAdmin,
        CancellationToken ct);
}