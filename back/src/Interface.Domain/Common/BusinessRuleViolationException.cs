namespace Interface.Domain.Common;

/// <summary>
/// Raised when a request is well-formed but breaks a business rule
/// (e.g. generating a payslip for an inactive employee).
/// </summary>
public sealed class BusinessRuleViolationException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
