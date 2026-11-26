namespace Interface.Domain.Payroll;

/// <summary>
/// One band of a progressive tax table. The band covers income above the previous bracket's
/// <see cref="UpperBound"/> up to and including its own. A null upper bound means "no limit".
/// <see cref="Rate"/> is a fraction: 0.10 means 10%.
/// </summary>
public sealed record TaxBracket(decimal? UpperBound, decimal Rate);
