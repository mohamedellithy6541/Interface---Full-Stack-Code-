namespace Interface.Domain.Payroll;

public interface ITaxCalculator
{
    decimal CalculateMonthlyTax(decimal taxableIncome, IReadOnlyList<TaxBracket> brackets);
}

public interface ISocialInsuranceCalculator
{
    decimal CalculateMonthlyContribution(decimal baseSalary, PayrollPolicy policy);
}

/// <summary>
/// Marginal (progressive) tax: each slice of income is taxed only at the rate of the bracket it falls in.
/// Income exactly on a boundary belongs to the lower bracket. The result is not rounded.
/// </summary>
public sealed class ProgressiveTaxCalculator : ITaxCalculator
{
    public decimal CalculateMonthlyTax(decimal taxableIncome, IReadOnlyList<TaxBracket> brackets)
    {
        if (taxableIncome <= 0)
            return 0m;

        var tax = 0m;
        var bracketLowerBound = 0m;

        foreach (var bracket in brackets)
        {
            if (taxableIncome <= bracketLowerBound)
                break;

            var bracketUpperBound = bracket.UpperBound ?? decimal.MaxValue;
            var incomeInBracket = Math.Min(taxableIncome, bracketUpperBound) - bracketLowerBound;
            tax += incomeInBracket * bracket.Rate;
            bracketLowerBound = bracketUpperBound;
        }

        return tax;
    }
}

/// <summary>Contribution = base salary x rate, limited to the monthly cap when one is configured. Not rounded.</summary>
public sealed class SocialInsuranceCalculator : ISocialInsuranceCalculator
{
    public decimal CalculateMonthlyContribution(decimal baseSalary, PayrollPolicy policy)
    {
        var contribution = baseSalary * policy.SocialInsuranceRate;
        return policy.SocialInsuranceMonthlyCap is { } cap
            ? Math.Min(contribution, cap)
            : contribution;
    }
}
