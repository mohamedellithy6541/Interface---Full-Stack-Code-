using Interface.Domain.Payroll;

namespace Interface.UnitTests.Domain;

public sealed class PayrollPolicyTests
{
    [Fact]
    public void Rejects_an_empty_bracket_table()
    {
        Assert.Throws<ArgumentException>(() => CreatePolicy([]));
    }

    [Fact]
    public void Rejects_a_table_whose_last_bracket_has_an_upper_bound()
    {
        Assert.Throws<ArgumentException>(() => CreatePolicy([new TaxBracket(10_000m, 0m)]));
    }

    [Fact]
    public void Rejects_upper_bounds_that_are_not_ascending()
    {
        Assert.Throws<ArgumentException>(() => CreatePolicy(
            [new TaxBracket(30_000m, 0m), new TaxBracket(10_000m, 0.1m), new TaxBracket(null, 0.2m)]));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Rejects_tax_rates_outside_zero_to_one(decimal rate)
    {
        Assert.Throws<ArgumentException>(() => CreatePolicy([new TaxBracket(null, rate)]));
    }

    [Fact]
    public void Rejects_a_negative_social_insurance_cap()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PayrollPolicy([new TaxBracket(null, 0.1m)], 0.11m, -1m, true, "EGP"));
    }

    private static PayrollPolicy CreatePolicy(IReadOnlyList<TaxBracket> brackets) =>
        new(brackets, 0.11m, null, true, "EGP");
}

public sealed class PayrollPeriodTests
{
    [Theory]
    [InlineData("2026-03", 2026, 3)]
    [InlineData("2024-12", 2024, 12)]
    public void Parses_year_and_month(string value, int year, int month)
    {
        Assert.True(PayrollPeriod.TryParse(value, out var period));
        Assert.Equal(new PayrollPeriod(year, month), period);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2026-13")]
    [InlineData("2026-3")]
    [InlineData("03-2026")]
    [InlineData("2026-03-01")]
    public void Rejects_anything_that_is_not_yyyy_MM(string? value)
    {
        Assert.False(PayrollPeriod.TryParse(value, out _));
    }

    [Fact]
    public void Knows_the_last_day_of_the_month_including_leap_years()
    {
        Assert.Equal(new DateOnly(2024, 2, 29), new PayrollPeriod(2024, 2).LastDay);
        Assert.Equal(new DateOnly(2026, 2, 28), new PayrollPeriod(2026, 2).LastDay);
    }
}
