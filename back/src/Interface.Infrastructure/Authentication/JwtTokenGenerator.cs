using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Interface.Application.Abstractions.Authentication;
using Interface.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Interface.Infrastructure.Authentication;

internal sealed class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider) : IJwtTokenGenerator
{
    public AccessToken Generate(User user)
    {
        var jwtOptions = options.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(jwtOptions.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role)
        };
        if (user.EmployeeId is { } employeeId)
            claims.Add(new Claim(AppClaimTypes.EmployeeId, employeeId.ToString()));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
