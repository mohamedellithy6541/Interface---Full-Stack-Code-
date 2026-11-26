namespace Interface.Application.Common.Exceptions;

/// <summary>Mapped to 404 Not Found.</summary>
public sealed class NotFoundException(string resourceName, object key)
    : Exception($"{resourceName} '{key}' was not found.");

/// <summary>Mapped to 409 Conflict (e.g. an email that is already taken).</summary>
public sealed class ConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Mapped to 401 Unauthorized (bad credentials, unknown external account).</summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);

/// <summary>Mapped to 403 Forbidden (authenticated, but not allowed to touch this resource).</summary>
public sealed class ForbiddenAccessException(string message = "You do not have permission to access this resource.")
    : Exception(message);
