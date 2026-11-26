using Interface.Domain.Common;
using Interface.Domain.Employees;

namespace Interface.Domain.Users;

/// <summary>An account that can sign in, either with a password or with Google.</summary>
public sealed class User : Entity
{
    public string Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string Role { get; private set; } = null!;

    /// <summary>Set for users with the Employee role: the employee record they may see.</summary>
    public Guid? EmployeeId { get; private set; }

    private User() { } // Required by EF Core.

    public static User Create(string email, string displayName, string role, Guid? employeeId = null)
    {
        if (!UserRole.All.Contains(role))
            throw new ArgumentException($"Unknown role '{role}'.", nameof(role));
        if (role == UserRole.Employee && employeeId is null)
            throw new ArgumentException("An Employee user must be linked to an employee record.", nameof(employeeId));

        return new User
        {
            Email = Employee.NormalizeEmail(email),
            DisplayName = displayName.Trim(),
            Role = role,
            EmployeeId = employeeId
        };
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
