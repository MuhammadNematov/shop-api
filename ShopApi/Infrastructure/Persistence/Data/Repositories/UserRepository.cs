using Application.ApiResult;
using Application.Identity;
using Application.Interfaces;
using Domain.Filters;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context)
    : IUserRepository
{
    public async Task<PagedResult<User>> GetPagedAsync(
        UserFilter filter,
        CancellationToken ct)
    {
        var query = context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();

            query = query.Where(u =>
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                u.Email!.ToLower().Contains(search));
        }

        return await query.ToPagedListAsync(
            filter.Page,
            filter.PageSize,
            ct);
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<int> UpdateAsync(User user)
    {
        context.Users.Update(user);

        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(User user)
    {
        context.Users.Remove(user);

        return await context.SaveChangesAsync();
    }

    public Task<bool> HasProductsAsync(int userId)
    {
        return context.Products
            .AnyAsync(p => p.CreatedByUserId == userId);
    }
}