using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategoryRepository(AppDbContext context)
    : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync(CancellationToken ct)
    {
        return context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct)
    {
        return context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<Category?> GetByNameAsync(
        string name,
        CancellationToken ct)
    {
        var normalizedName = name.Trim().ToLower();

        return context.Categories
            .FirstOrDefaultAsync(
                c => c.Name.ToLower() == normalizedName,
                ct);
    }

    public Task<bool> HasProductsAsync(int id, CancellationToken ct)
    {
        return context.Products
            .AnyAsync(p => p.CategoryId == id, ct);
    }

    public async Task<int> CreateAsync(Category category)
    {
        context.Categories.Add(category);

        await context.SaveChangesAsync();

        return category.Id;
    }

    public async Task<int> UpdateAsync(Category category)
    {
        context.Categories.Update(category);

        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Category category)
    {
        context.Categories.Remove(category);

        return await context.SaveChangesAsync();
    }
}