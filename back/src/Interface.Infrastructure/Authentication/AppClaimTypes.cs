using System.IdentityModel.Tokens.Jwt;

namespace Interface.Infrastructure.Authentication;

/// <summary>Short JWT claim names written into our tokens and read back by the API (inbound claim mapping is off).</summary>
public static class AppClaimTypes
{
    public const string UserId = JwtRegisteredClaimNames.Sub;
    public const string Email = JwtRegisteredClaimNames.Email;
    public const string Name = JwtRegisteredClaimNames.Name;
    public const string Role = "role";
    public const string EmployeeId = "employee_id";
}
