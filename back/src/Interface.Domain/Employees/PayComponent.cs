namespace Interface.Domain.Employees;

/// <summary>A named fixed monthly amount: an allowance (adds to gross) or a deduction (subtracted from net).</summary>
public sealed record PayComponent
{
    public string Name { get; }
    public decimal Amount { get; }

    public PayComponent(string name, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pay component name is required.", nameof(name));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Pay component amount cannot be negative.");

        Name = name.Trim();
        Amount = amount;
    }
}
