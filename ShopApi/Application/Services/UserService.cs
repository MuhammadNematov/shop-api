using Application.ApiResult;
using Application.DTOs.Users;
using Application.Identity;
using Application.Interfaces;
using Domain.Filters;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository,
    UserManager<User> userManager)
    : IUserService
{
    public async Task<Result<PagedResult<UserResponseDto>>> GetPagedAsync(
        UserFilter filter,
        CancellationToken ct)
    {
        var users = await userRepository.GetPagedAsync(filter, ct);

        var items = new List<UserResponseDto>();

        foreach (var user in users.Items)
        {
            var roles = await userManager.GetRolesAsync(user);

            items.Add(new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                Role = roles.FirstOrDefault() ?? "User"
            });
        }

        var result = new PagedResult<UserResponseDto>
        {
            Items = items,
            Page = users.Page,
            PageSize = users.PageSize,
            TotalCount = users.TotalCount
        };

        return Result<PagedResult<UserResponseDto>>.Success(result);
    }

    public async Task<Result<UserResponseDto>> GetByIdAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return Result<UserResponseDto>.Fail(
                "User not found.",
                ErrorType.NotFound);
        }

        var roles = await userManager.GetRolesAsync(user);

        var result = new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            Role = roles.FirstOrDefault() ?? "User"
        };

        return Result<UserResponseDto>.Success(result);
    }

    public async Task<Result<UserResponseDto>> UpdateAsync(
        int id,
        UpdateUserDto dto)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return Result<UserResponseDto>.Fail(
                "User not found.",
                ErrorType.NotFound);
        }

        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            dto.FirstName.Trim().Length > 100)
        {
            return Result<UserResponseDto>.Fail(
                "First name is required and cannot exceed 100 characters.",
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(dto.LastName) ||
            dto.LastName.Trim().Length > 100)
        {
            return Result<UserResponseDto>.Fail(
                "Last name is required and cannot exceed 100 characters.",
                ErrorType.Validation);
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.BirthDate = dto.BirthDate;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                updateResult.Errors.Select(e => e.Description));

            return Result<UserResponseDto>.Fail(
                errors,
                ErrorType.Validation);
        }

        var roles = await userManager.GetRolesAsync(user);

        var result = new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            Role = roles.FirstOrDefault() ?? "User"
        };

        return Result<UserResponseDto>.Success(result);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return Result<bool>.Fail(
                "User not found.",
                ErrorType.NotFound);
        }

        var hasProducts = await userRepository.HasProductsAsync(id);

        if (hasProducts)
        {
            return Result<bool>.Fail(
                "Cannot delete a user who has created products.",
                ErrorType.Conflict);
        }

        var deleteResult = await userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                deleteResult.Errors.Select(e => e.Description));

            return Result<bool>.Fail(
                errors,
                ErrorType.Validation);
        }

        return Result<bool>.Success(true);
    }
}