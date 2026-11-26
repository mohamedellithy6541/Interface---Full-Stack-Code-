using AutoMapper;
using Interface.Domain.Users;

namespace Interface.Application.Features.Auth;

public sealed record CurrentUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid? EmployeeId { get; init; }
}

public sealed record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTimeOffset ExpiresAtUtc { get; init; }
    public CurrentUserDto User { get; init; } = new();
}

public sealed class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, CurrentUserDto>();
    }
}
