using System.Globalization;

namespace Interface.Domain.Payroll;

/// <summary>A calendar month a payslip is issued for, written as "yyyy-MM".</summary>
public readonly record struct PayrollPeriod(int Year, int Month)
{
    public const string Format = "yyyy-MM";

    public DateOnly FirstDay => new(Year, Month, 1);
    public DateOnly LastDay => FirstDay.AddMonths(1).AddDays(-1);

    public static bool TryParse(string? value, out PayrollPeriod period)
    {
        period = default;
        if (!DateOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return false;

        period = new PayrollPeriod(date.Year, date.Month);
        return true;
    }

    public static PayrollPeriod Parse(string value) =>
        TryParse(value, out var period)
            ? period
            : throw new FormatException($"'{value}' is not a valid payroll month. Expected format: {Format}.");

    public override string ToString() => FirstDay.ToString(Format, CultureInfo.InvariantCulture);
}
