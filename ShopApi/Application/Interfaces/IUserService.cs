using Application.ApiResult;
using Application.DTOs.Users;
using Domain.Filters;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Result<PagedResult<UserResponseDto>>> GetPagedAsync(
        UserFilter filter,
        CancellationToken ct);

    Task<Result<UserResponseDto>> GetByIdAsync(int id);

    Task<Result<UserResponseDto>> UpdateAsync(
        int id,
        UpdateUserDto dto);

    Task<Result<bool>> DeleteAsync(int id);
}