using Interface.Domain.Common;
using Interface.Domain.Employees;

namespace Interface.Domain.Payroll;

public interface IPayrollCalculator
{
    Payslip Calculate(Employee employee, PayrollPeriod period, PayrollPolicy policy);
}

public static class PayrollErrorCodes
{
    public const string EmployeeInactive = "payroll.employee_inactive";
    public const string PeriodBeforeHireDate = "payroll.period_before_hire_date";
    public const string DeductionsExceedGrossPay = "payroll.deductions_exceed_gross_pay";
}

/// <summary>
/// Builds a monthly payslip. Every component is rounded to 2 decimals (half away from zero) on its own,
/// then totals are summed from the rounded components, so the lines on a payslip always add up exactly.
/// </summary>
public sealed class PayrollCalculator(
    ITaxCalculator taxCalculator,
    ISocialInsuranceCalculator socialInsuranceCalculator) : IPayrollCalculator
{
    public Payslip Calculate(Employee employee, PayrollPeriod period, PayrollPolicy policy)
    {
        EnsureEmployeeCanBePaid(employee, period);

        var baseSalary = Money.Round(employee.BaseMonthlySalary);
        var allowances = ToRoundedLines(employee.Allowances);
        var grossPay = baseSalary + allowances.Sum(line => line.Amount);

        var socialInsurance = Money.Round(
            socialInsuranceCalculator.CalculateMonthlyContribution(baseSalary, policy));

        var taxableIncome = policy.SocialInsuranceReducesTaxableIncome
            ? Math.Max(0m, grossPay - socialInsurance)
            : grossPay;
        var incomeTax = Money.Round(taxCalculator.CalculateMonthlyTax(taxableIncome, policy.TaxBrackets));

        var otherDeductions = ToRoundedLines(employee.Deductions);
        var totalDeductions = incomeTax + socialInsurance + otherDeductions.Sum(line => line.Amount);
        var netPay = grossPay - totalDeductions;

        if (netPay < 0)
            throw new BusinessRuleViolationException(
                PayrollErrorCodes.DeductionsExceedGrossPay,
                $"Total deductions ({totalDeductions}) exceed gross pay ({grossPay}) for {period}.");

        return new Payslip(
            employee.Id,
            employee.FullName,
            period,
            policy.Currency,
            baseSalary,
            allowances,
            grossPay,
            taxableIncome,
            incomeTax,
            socialInsurance,
            otherDeductions,
            totalDeductions,
            netPay);
    }

    private static void EnsureEmployeeCanBePaid(Employee employee, PayrollPeriod period)
    {
        if (!employee.IsActive)
            throw new BusinessRuleViolationException(
                PayrollErrorCodes.EmployeeInactive,
                "A payslip cannot be generated for an inactive employee.");

        if (period.LastDay < employee.HireDate)
            throw new BusinessRuleViolationException(
                PayrollErrorCodes.PeriodBeforeHireDate,
                $"The employee was hired on {employee.HireDate:yyyy-MM-dd}, after the payroll month {period}.");
    }

    private static List<PayslipLine> ToRoundedLines(IEnumerable<PayComponent> components) =>
        components.Select(component => new PayslipLine(component.Name, Money.Round(component.Amount))).ToList();
}
