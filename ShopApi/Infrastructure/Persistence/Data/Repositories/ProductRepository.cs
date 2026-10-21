using Application.ApiResult;
using Application.Interfaces;
using Domain.Entities;
using Domain.Filters;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext context)
    : IProductRepository
{
    public async Task<PagedResult<Product>> GetPagedAsync(
        ProductFilter filter,
        CancellationToken ct)
    {
        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();

            query = query.Where(p =>
                p.Name.ToLower().Contains(search));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p =>
                p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p =>
                p.Price <= filter.MaxPrice.Value);
        }

        if (filter.OnlyActive)
        {
            query = query.Where(p => p.IsActive);
        }

        query = filter.SortBy.ToLower() switch
        {
            "price" => filter.Desc
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),

            "createdat" => filter.Desc
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),

            "stock" => filter.Desc
                ? query.OrderByDescending(p => p.Stock)
                : query.OrderBy(p => p.Stock),

            _ => filter.Desc
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name)
        };

        return await query.ToPagedListAsync(
            filter.Page,
            filter.PageSize,
            ct);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        return context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public Task<Product?> GetByNameInCategoryAsync(
        string name,
        int categoryId,
        CancellationToken ct)
    {
        var normalizedName = name.Trim().ToLower();

        return context.Products
            .FirstOrDefaultAsync(
                p => p.CategoryId == categoryId &&
                     p.Name.ToLower() == normalizedName,
                ct);
    }

    public async Task<int> CreateAsync(Product product)
    {
        context.Products.Add(product);

        await context.SaveChangesAsync();

        return product.Id;
    }

    public async Task<int> UpdateAsync(Product product)
    {
        context.Products.Update(product);

        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Product product)
    {
        context.Products.Remove(product);

        return await context.SaveChangesAsync();
    }
}