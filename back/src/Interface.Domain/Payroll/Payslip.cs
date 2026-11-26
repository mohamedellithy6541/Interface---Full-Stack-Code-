namespace Interface.Domain.Payroll;

public sealed record PayslipLine(string Name, decimal Amount);

public sealed record Payslip(
    Guid EmployeeId,
    string EmployeeName,
    PayrollPeriod Period,
    string Currency,
    decimal BaseSalary,
    IReadOnlyList<PayslipLine> Allowances,
    decimal GrossPay,
    decimal TaxableIncome,
    decimal IncomeTax,
    decimal SocialInsurance,
    IReadOnlyList<PayslipLine> OtherDeductions,
    decimal TotalDeductions,
    decimal NetPay);
