using System.ComponentModel.DataAnnotations;

namespace Interface.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumSigningKeyLength = 32;

    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string Audience { get; set; } = string.Empty;

    /// <summary>HMAC-SHA256 key, at least 32 characters. Never commit it: use user-secrets or an environment variable.</summary>
    [Required, MinLength(MinimumSigningKeyLength)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 24 * 60)] public int ExpiryMinutes { get; set; } = 60;
}

public sealed class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    /// <summary>OAuth 2.0 Web client ID from Google Cloud Console. Empty disables Google sign-in.</summary>
    public string? ClientId { get; set; }
}
