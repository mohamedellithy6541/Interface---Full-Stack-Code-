using Interface.Domain.Common;

namespace Interface.Domain.Employees;

public sealed class Employee : Entity, IAuditable
{
    public const int FullNameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int JobTitleMaxLength = 100;
    public const int DepartmentMaxLength = 100;

    private readonly List<PayComponent> _allowances = [];
    private readonly List<PayComponent> _deductions = [];

    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string JobTitle { get; private set; } = null!;
    public string Department { get; private set; } = null!;
    public DateOnly HireDate { get; private set; }
    public decimal BaseMonthlySalary { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public IReadOnlyCollection<PayComponent> Allowances => _allowances.AsReadOnly();
    public IReadOnlyCollection<PayComponent> Deductions => _deductions.AsReadOnly();

    public bool IsActive => Status == EmployeeStatus.Active;

    private Employee() { } // Required by EF Core.

    public static Employee Create(
        string fullName,
        string email,
        string jobTitle,
        string department,
        DateOnly hireDate,
        decimal baseMonthlySalary,
        EmployeeStatus status,
        IEnumerable<PayComponent>? allowances = null,
        IEnumerable<PayComponent>? deductions = null)
    {
        var employee = new Employee();
        employee.UpdateDetails(fullName, email, jobTitle, department, hireDate, baseMonthlySalary, status);
        employee.ReplaceAllowances(allowances ?? []);
        employee.ReplaceDeductions(deductions ?? []);
        return employee;
    }

    public void UpdateDetails(
        string fullName,
        string email,
        string jobTitle,
        string department,
        DateOnly hireDate,
        decimal baseMonthlySalary,
        EmployeeStatus status)
    {
        if (baseMonthlySalary <= 0)
            throw new ArgumentOutOfRangeException(nameof(baseMonthlySalary), "Base monthly salary must be greater than zero.");

        FullName = fullName.Trim();
        Email = NormalizeEmail(email);
        JobTitle = jobTitle.Trim();
        Department = department.Trim();
        HireDate = hireDate;
        BaseMonthlySalary = baseMonthlySalary;
        Status = status;
    }

    public void ReplaceAllowances(IEnumerable<PayComponent> allowances)
    {
        _allowances.Clear();
        _allowances.AddRange(allowances);
    }

    public void ReplaceDeductions(IEnumerable<PayComponent> deductions)
    {
        _deductions.Clear();
        _deductions.AddRange(deductions);
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
