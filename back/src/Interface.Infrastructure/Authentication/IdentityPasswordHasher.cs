using Interface.Domain.Users;
using Microsoft.AspNetCore.Identity;
using IPasswordHasher = Interface.Application.Abstractions.Authentication.IPasswordHasher;

namespace Interface.Infrastructure.Authentication;

/// <summary>Uses ASP.NET Core Identity's PBKDF2 hasher instead of a home-made password hash.</summary>
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        user.PasswordHash is not null
        && _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
