using Interface.Domain.Employees;
using Interface.Domain.Payroll;

namespace Interface.UnitTests.TestData;

internal sealed class EmployeeBuilder
{
    private decimal _baseSalary = 20_000m;
    private EmployeeStatus _status = EmployeeStatus.Active;
    private DateOnly _hireDate = new(2020, 1, 15);
    private readonly List<PayComponent> _allowances = [];
    private readonly List<PayComponent> _deductions = [];

    public EmployeeBuilder WithBaseSalary(decimal baseSalary) { _baseSalary = baseSalary; return this; }
    public EmployeeBuilder Inactive() { _status = EmployeeStatus.Inactive; return this; }
    public EmployeeBuilder HiredOn(DateOnly hireDate) { _hireDate = hireDate; return this; }
    public EmployeeBuilder WithAllowance(string name, decimal amount) { _allowances.Add(new PayComponent(name, amount)); return this; }
    public EmployeeBuilder WithDeduction(string name, decimal amount) { _deductions.Add(new PayComponent(name, amount)); return this; }

    public Employee Build() => Employee.Create(
        "Test Employee", "test@interface.local", "Engineer", "Engineering",
        _hireDate, _baseSalary, _status, _allowances, _deductions);
}

internal static class TestPolicies
{
    /// <summary>0% up to 10,000; 10% to 30,000; 15% to 50,000; 20% above. Insurance 11% capped at 1,500.</summary>
    public static PayrollPolicy Standard(
        decimal socialInsuranceRate = 0.11m,
        decimal? socialInsuranceCap = 1_500m,
        bool socialInsuranceReducesTaxableIncome = true) =>
        new(
            [new TaxBracket(10_000m, 0m), new TaxBracket(30_000m, 0.10m), new TaxBracket(50_000m, 0.15m), new TaxBracket(null, 0.20m)],
            socialInsuranceRate,
            socialInsuranceCap,
            socialInsuranceReducesTaxableIncome,
            "EGP");
}
