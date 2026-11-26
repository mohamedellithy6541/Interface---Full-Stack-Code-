using Interface.Application.Abstractions.Authentication;
using Interface.Domain.Users;
using Interface.Infrastructure.Authentication;

namespace Interface.Api.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private System.Security.Claims.ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId => ReadGuid(AppClaimTypes.UserId);

    public string? Role => Principal?.FindFirst(AppClaimTypes.Role)?.Value;

    public Guid? EmployeeId => ReadGuid(AppClaimTypes.EmployeeId);

    public bool IsHr => Role == UserRole.Hr;

    private Guid? ReadGuid(string claimType) =>
        Guid.TryParse(Principal?.FindFirst(claimType)?.Value, out var value) ? value : null;
}
