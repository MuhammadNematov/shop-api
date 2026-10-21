using Application.Identity;

namespace Application.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(User user);
}