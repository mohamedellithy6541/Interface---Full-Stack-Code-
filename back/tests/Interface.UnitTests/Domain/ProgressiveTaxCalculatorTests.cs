using Interface.Domain.Payroll;
using Interface.UnitTests.TestData;

namespace Interface.UnitTests.Domain;

public sealed class ProgressiveTaxCalculatorTests
{
    private readonly ProgressiveTaxCalculator _calculator = new();
    private readonly IReadOnlyList<TaxBracket> _brackets = TestPolicies.Standard().TaxBrackets;

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-500, 0)]
    [InlineData(5_000, 0)]
    [InlineData(10_000, 0)]          // exactly on the first boundary: still 0%
    [InlineData(10_000.01, 0.001)]   // one cent into the 10% bracket
    [InlineData(20_000, 1_000)]
    [InlineData(30_000, 2_000)]      // exactly on the second boundary
    [InlineData(50_000, 5_000)]      // 2,000 + 20,000 x 15%
    [InlineData(60_000, 7_000)]      // 5,000 + 10,000 x 20%
    public void Taxes_each_slice_of_income_at_its_own_bracket_rate(decimal taxableIncome, decimal expectedTax)
    {
        var tax = _calculator.CalculateMonthlyTax(taxableIncome, _brackets);

        Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public void A_single_unbounded_bracket_is_a_flat_tax()
    {
        var tax = _calculator.CalculateMonthlyTax(12_345m, [new TaxBracket(null, 0.10m)]);

        Assert.Equal(1_234.5m, tax);
    }
}
