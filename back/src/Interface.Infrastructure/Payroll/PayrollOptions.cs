using Interface.Domain.Payroll;

namespace Interface.Infrastructure.Payroll;

/// <summary>The "Payroll" configuration section. Rates are fractions: 0.11 means 11%.</summary>
public sealed class PayrollOptions
{
    public const string SectionName = "Payroll";

    public string Currency { get; set; } = "EGP";
    public List<TaxBracketOptions> TaxBrackets { get; set; } = [];
    public decimal SocialInsuranceRate { get; set; }
    public decimal? SocialInsuranceMonthlyCap { get; set; }
    public bool SocialInsuranceReducesTaxableIncome { get; set; } = true;

    public PayrollPolicy ToPolicy() => new(
        TaxBrackets.Select(bracket => new TaxBracket(bracket.UpTo, bracket.Rate)).ToList(),
        SocialInsuranceRate,
        SocialInsuranceMonthlyCap,
        SocialInsuranceReducesTaxableIncome,
        Currency);
}

public sealed class TaxBracketOptions
{
    /// <summary>Upper bound of the bracket (inclusive). Leave empty on the last bracket.</summary>
    public decimal? UpTo { get; set; }
    public decimal Rate { get; set; }
}
