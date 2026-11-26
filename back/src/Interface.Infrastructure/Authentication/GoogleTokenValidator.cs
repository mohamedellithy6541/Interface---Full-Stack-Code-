using Google.Apis.Auth;
using Interface.Application.Abstractions.Authentication;
using Interface.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Interface.Infrastructure.Authentication;

/// <summary>Verifies a Google ID token's signature, expiry, issuer and audience (our client ID).</summary>
internal sealed class GoogleTokenValidator(
    IOptions<GoogleAuthOptions> options,
    ILogger<GoogleTokenValidator> logger) : IGoogleTokenValidator
{
    public async Task<ExternalUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
            throw new AuthenticationFailedException("Google sign-in is not configured on the server.");

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });

            return new ExternalUserInfo(payload.Email, payload.Name, payload.EmailVerified);
        }
        catch (InvalidJwtException exception)
        {
            logger.LogWarning("Rejected Google ID token: {Reason}", exception.Message);
            return null;
        }
    }
}
