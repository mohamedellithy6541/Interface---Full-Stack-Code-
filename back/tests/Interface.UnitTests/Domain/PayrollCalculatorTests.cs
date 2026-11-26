using Interface.Domain.Common;
using Interface.Domain.Payroll;
using Interface.UnitTests.TestData;

namespace Interface.UnitTests.Domain;

public sealed class PayrollCalculatorTests
{
    private static readonly PayrollPeriod March2026 = new(2026, 3);

    private readonly PayrollCalculator _calculator = new(new ProgressiveTaxCalculator(), new SocialInsuranceCalculator());

    [Fact]
    public void Calculates_a_full_payslip_with_allowances_and_deductions()
    {
        var employee = new EmployeeBuilder()
            .WithBaseSalary(25_000m)
            .WithAllowance("Transport", 1_500m)
            .WithAllowance("Housing", 3_000m)
            .WithDeduction("Medical plan", 400m)
            .Build();

        var payslip = _calculator.Calculate(employee, March2026, TestPolicies.Standard());

        Assert.Equal(29_500m, payslip.GrossPay);
        Assert.Equal(1_500m, payslip.SocialInsurance);   // 25,000 x 11% = 2,750, capped at 1,500
        Assert.Equal(28_000m, payslip.TaxableIncome);    // gross - social insurance
        Assert.Equal(1_800m, payslip.IncomeTax);         // (28,000 - 10,000) x 10%
        Assert.Equal(3_700m, payslip.TotalDeductions);
        Assert.Equal(25_800m, payslip.NetPay);
        Assert.Equal("2026-03", payslip.Period.ToString());
        Assert.Equal("EGP", payslip.Currency);
    }

    [Fact]
    public void Zero_allowances_means_gross_equals_base_salary()
    {
        var employee = new EmployeeBuilder().WithBaseSalary(8_000m).Build();

        var payslip = _calculator.Calculate(employee, March2026, TestPolicies.Standard());

        Assert.Empty(payslip.Allowances);
        Assert.Equal(8_000m, payslip.GrossPay);
        Assert.Equal(880m, payslip.SocialInsurance);     // under the cap
        Assert.Equal(0m, payslip.IncomeTax);             // 7,120 taxable is inside the 0% bracket
        Assert.Equal(7_120m, payslip.NetPay);
    }

    [Fact]
    public void Social_insurance_is_capped_when_a_cap_is_configured()
    {
        var employee = new EmployeeBuilder().WithBaseSalary(100_000m).Build();

        var capped = _calculator.Calculate(employee, March2026, TestPolicies.Standard(socialInsuranceCap: 1_500m));
        var uncapped = _calculator.Calculate(employee, March2026, TestPolicies.Standard(socialInsuranceCap: null));

        Assert.Equal(1_500m, capped.SocialInsurance);
        Assert.Equal(11_000m, uncapped.SocialInsurance);
    }

    [Fact]
    public void Social_insurance_is_calculated_on_base_salary_only_not_allowances()
    {
        var employee = new EmployeeBuilder().WithBaseSalary(10_000m).WithAllowance("Housing", 5_000m).Build();

        var payslip = _calculator.Calculate(employee, March2026, TestPolicies.Standard(socialInsuranceCap: null));

        Assert.Equal(1_100m, payslip.SocialInsurance);
    }

    [Fact]
    public void Taxable_income_is_gross_when_social_insurance_does_not_reduce_it()
    {
        var employee = new EmployeeBuilder().WithBaseSalary(20_000m).Build();

        var payslip = _calculator.Calculate(
            employee, March2026, TestPolicies.Standard(socialInsuranceReducesTaxableIncome: false));

        Assert.Equal(20_000m, payslip.TaxableIncome);
        Assert.Equal(1_000m, payslip.IncomeTax);
    }

    [Fact]
    public void Taxable_income_exactly_on_a_bracket_boundary_is_taxed_at_the_lower_bracket()
    {
        // 10,000 base, no insurance: taxable income lands exactly on the 0% / 10% boundary.
        var employee = new EmployeeBuilder().WithBaseSalary(10_000m).Build();

        var payslip = _calculator.Calculate(employee, March2026, TestPolicies.Standard(socialInsuranceRate: 0m));

        Assert.Equal(10_000m, payslip.TaxableIncome);
        Assert.Equal(0m, payslip.IncomeTax);
    }

    [Fact]
    public void Amounts_are_rounded_to_two_decimals_half_away_from_zero()
    {
        // Taxable 10,000.05 -> raw tax 0.005 -> rounds up to 0.01 (banker's rounding would give 0.00).
        var employee = new EmployeeBuilder().WithBaseSalary(10_000.05m).Build();

        var payslip = _calculator.Calculate(employee, March2026, TestPolicies.Standard(socialInsuranceRate: 0m));

        Assert.Equal(0.01m, payslip.IncomeTax);
        Assert.Equal(10_000.04m, payslip.NetPay);
    }

    [Fact]
    public void Payslip_lines_always_add_up_exactly_to_the_totals()
    {
        var employee = new EmployeeBuilder()
            .WithBaseSalary(33_333.33m)
            .WithAllowance("Transport", 1_234.565m)
            .WithDeduction("Loan", 99.995m)
            .Build();

        var payslip = _calculator.Calculate(employee, March2026, TestPolicies.Standard(socialInsuranceCap: null));

        Assert.Equal(payslip.BaseSalary + payslip.Allowances.Sum(line => line.Amount), payslip.GrossPay);
        Assert.Equal(
            payslip.IncomeTax + payslip.SocialInsurance + payslip.OtherDeductions.Sum(line => line.Amount),
            payslip.TotalDeductions);
        Assert.Equal(payslip.GrossPay - payslip.TotalDeductions, payslip.NetPay);
        Assert.All(
            new[] { payslip.GrossPay, payslip.IncomeTax, payslip.SocialInsurance, payslip.NetPay },
            amount => Assert.Equal(amount, Money.Round(amount)));
    }

    [Fact]
    public void Inactive_employee_cannot_be_paid()
    {
        var employee = new EmployeeBuilder().Inactive().Build();

        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => _calculator.Calculate(employee, March2026, TestPolicies.Standard()));

        Assert.Equal(PayrollErrorCodes.EmployeeInactive, exception.Code);
    }

    [Fact]
    public void Month_before_the_hire_month_is_rejected()
    {
        var employee = new EmployeeBuilder().HiredOn(new DateOnly(2026, 4, 1)).Build();

        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => _calculator.Calculate(employee, March2026, TestPolicies.Standard()));

        Assert.Equal(PayrollErrorCodes.PeriodBeforeHireDate, exception.Code);
    }

    [Fact]
    public void Employee_hired_during_the_month_is_paid_for_that_month()
    {
        var employee = new EmployeeBuilder().HiredOn(new DateOnly(2026, 3, 31)).Build();

        var payslip = _calculator.Calculate(employee, March2026, TestPolicies.Standard());

        Assert.True(payslip.NetPay > 0);
    }

    [Fact]
    public void Deductions_larger_than_gross_pay_are_rejected_instead_of_producing_a_negative_net()
    {
        var employee = new EmployeeBuilder().WithBaseSalary(5_000m).WithDeduction("Loan", 10_000m).Build();

        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => _calculator.Calculate(employee, March2026, TestPolicies.Standard()));

        Assert.Equal(PayrollErrorCodes.DeductionsExceedGrossPay, exception.Code);
    }
}
