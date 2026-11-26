namespace Interface.Domain.Payroll;

/// <summary>The configurable rules a payslip is calculated with. Built from configuration, never hard-coded.</summary>
public sealed record PayrollPolicy
{
    public IReadOnlyList<TaxBracket> TaxBrackets { get; }
    public decimal SocialInsuranceRate { get; }
    public decimal? SocialInsuranceMonthlyCap { get; }
    public bool SocialInsuranceReducesTaxableIncome { get; }
    public string Currency { get; }

    public PayrollPolicy(
        IReadOnlyList<TaxBracket> taxBrackets,
        decimal socialInsuranceRate,
        decimal? socialInsuranceMonthlyCap,
        bool socialInsuranceReducesTaxableIncome,
        string currency)
    {
        EnsureValidBrackets(taxBrackets);
        if (socialInsuranceRate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(socialInsuranceRate), "Social insurance rate must be between 0 and 1.");
        if (socialInsuranceMonthlyCap is < 0)
            throw new ArgumentOutOfRangeException(nameof(socialInsuranceMonthlyCap), "Social insurance cap cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        TaxBrackets = taxBrackets;
        SocialInsuranceRate = socialInsuranceRate;
        SocialInsuranceMonthlyCap = socialInsuranceMonthlyCap;
        SocialInsuranceReducesTaxableIncome = socialInsuranceReducesTaxableIncome;
        Currency = currency;
    }

    private static void EnsureValidBrackets(IReadOnlyList<TaxBracket> brackets)
    {
        if (brackets.Count == 0)
            throw new ArgumentException("At least one tax bracket is required.", nameof(brackets));
        if (brackets[^1].UpperBound is not null)
            throw new ArgumentException("The last tax bracket must have no upper bound.", nameof(brackets));

        var previousUpperBound = 0m;
        for (var i = 0; i < brackets.Count; i++)
        {
            var bracket = brackets[i];
            if (bracket.Rate is < 0 or > 1)
                throw new ArgumentException($"Tax bracket {i + 1} rate must be between 0 and 1.", nameof(brackets));

            if (i == brackets.Count - 1)
                continue;

            if (bracket.UpperBound is not { } upperBound || upperBound <= previousUpperBound)
                throw new ArgumentException("Tax bracket upper bounds must be set and strictly ascending.", nameof(brackets));
            previousUpperBound = upperBound;
        }
    }
}
