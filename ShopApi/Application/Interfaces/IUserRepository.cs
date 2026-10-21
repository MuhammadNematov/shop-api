using Application.ApiResult;
using Application.Identity;
using Domain.Filters;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<PagedResult<User>> GetPagedAsync(
        UserFilter filter,
        CancellationToken ct);

    Task<User?> GetByIdAsync(int id);
    Task<int> UpdateAsync(User user);
    Task<int> DeleteAsync(User user);
    Task<bool> HasProductsAsync(int userId);
}