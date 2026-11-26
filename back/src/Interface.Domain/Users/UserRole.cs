namespace Interface.Domain.Users;

public static class UserRole
{
    public const string Hr = "HR";
    public const string Employee = "Employee";

    public static readonly IReadOnlyCollection<string> All = [Hr, Employee];
}
