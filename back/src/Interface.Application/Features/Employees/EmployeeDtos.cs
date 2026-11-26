using Interface.Domain.Employees;

namespace Interface.Application.Features.Employees;

public sealed record PayComponentDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public sealed record EmployeeDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public DateOnly HireDate { get; init; }
    public decimal BaseMonthlySalary { get; init; }
    public EmployeeStatus Status { get; init; }
    public IReadOnlyList<PayComponentDto> Allowances { get; init; } = [];
    public IReadOnlyList<PayComponentDto> Deductions { get; init; } = [];
}

/// <summary>The editable fields shared by the create and update commands, so both use one set of validation rules.</summary>
public interface IEmployeeInput
{
    string FullName { get; }
    string Email { get; }
    string JobTitle { get; }
    string Department { get; }
    DateOnly HireDate { get; }
    decimal BaseMonthlySalary { get; }
    EmployeeStatus Status { get; }
    IReadOnlyList<PayComponentDto> Allowances { get; }
    IReadOnlyList<PayComponentDto> Deductions { get; }
}
