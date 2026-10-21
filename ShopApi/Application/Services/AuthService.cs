using Application.ApiResult;
using Application.DTOs.Auth;
using Application.Identity;
using Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class AuthService(
    UserManager<User> userManager,
    IJwtTokenGenerator jwtTokenGenerator)
    : IAuthService
{
    public async Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName))
        {
            return Result<AuthResponseDto>.Fail(
                "First name and last name are required.",
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(dto.Email) ||
            !dto.Email.Contains('@'))
        {
            return Result<AuthResponseDto>.Fail(
                "A valid email is required.",
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(dto.Password) ||
            dto.Password.Length < 6)
        {
            return Result<AuthResponseDto>.Fail(
                "Password must contain at least 6 characters.",
                ErrorType.Validation);
        }

        var existingUser = await userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return Result<AuthResponseDto>.Fail(
                "A user with this email already exists.",
                ErrorType.Conflict);
        }

        var user = new User
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            BirthDate = dto.BirthDate
        };

        var createResult = await userManager.CreateAsync(
            user, dto.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(e => e.Description));

            return Result<AuthResponseDto>.Fail(
                errors,
                ErrorType.Validation);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            return Result<AuthResponseDto>.Fail(
                "Could not assign the user role. Make sure roles are seeded.",
                ErrorType.Validation);
        }

        var token = await jwtTokenGenerator.GenerateTokenAsync(user);

        var response = new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            Token = token,
            Role = "User"
        };

        return Result<AuthResponseDto>.Success(response);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return Result<AuthResponseDto>.Fail(
                "Email and password are required.",
                ErrorType.Validation);
        }

        var user = await userManager.FindByEmailAsync(dto.Email.Trim());

        if (user is null)
        {
            return Result<AuthResponseDto>.Fail(
                "Invalid email or password.",
                ErrorType.Unauthorized);
        }

        var passwordValid = await userManager.CheckPasswordAsync(
            user, dto.Password);

        if (!passwordValid)
        {
            return Result<AuthResponseDto>.Fail(
                "Invalid email or password.",
                ErrorType.Unauthorized);
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var token = await jwtTokenGenerator.GenerateTokenAsync(user);

        var response = new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            Token = token,
            Role = role
        };

        return Result<AuthResponseDto>.Success(response);
    }
}