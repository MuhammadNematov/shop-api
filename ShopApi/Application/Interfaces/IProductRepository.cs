using Application.ApiResult;
using Domain.Entities;
using Domain.Filters;

namespace Application.Interfaces;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetPagedAsync(
        ProductFilter filter,
        CancellationToken ct);

    Task<Product?> GetByIdAsync(int id, CancellationToken ct);

    Task<Product?> GetByNameInCategoryAsync(
        string name,
        int categoryId,
        CancellationToken ct);

    Task<int> CreateAsync(Product product);
    Task<int> UpdateAsync(Product product);
    Task<int> DeleteAsync(Product product);
}