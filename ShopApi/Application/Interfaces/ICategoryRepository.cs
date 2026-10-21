using Domain.Entities;

namespace Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);
    Task<Category?> GetByNameAsync(string name, CancellationToken ct);
    Task<bool> HasProductsAsync(int id, CancellationToken ct);
    Task<int> CreateAsync(Category category);
    Task<int> UpdateAsync(Category category);
    Task<int> DeleteAsync(Category category);
}