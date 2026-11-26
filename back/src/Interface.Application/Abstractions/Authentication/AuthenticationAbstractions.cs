using Interface.Domain.Users;

namespace Interface.Application.Abstractions.Authentication;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    AccessToken Generate(User user);
}

public interface IPasswordHasher
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}

public sealed record ExternalUserInfo(string Email, string? Name, bool EmailVerified);

public interface IGoogleTokenValidator
{
    /// <summary>Returns the verified Google user, or null when the ID token is invalid or expired.</summary>
    Task<ExternalUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken);
}

/// <summary>The authenticated caller of the current request.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Role { get; }
    Guid? EmployeeId { get; }
    bool IsHr { get; }
}
