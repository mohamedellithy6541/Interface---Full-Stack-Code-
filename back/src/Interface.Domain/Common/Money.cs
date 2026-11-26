namespace Interface.Domain.Common;

/// <summary>
/// Money is held as <see cref="decimal"/> (base-10, exact for currency amounts), never as double/float.
/// Every amount shown on a payslip is rounded to 2 decimals using "round half away from zero".
/// </summary>
public static class Money
{
    public const int Decimals = 2;

    public static decimal Round(decimal amount) =>
        Math.Round(amount, Decimals, MidpointRounding.AwayFromZero);
}
