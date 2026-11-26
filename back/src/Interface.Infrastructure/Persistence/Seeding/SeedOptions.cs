namespace Interface.Infrastructure.Persistence.Seeding;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    /// <summary>Insert demo employees and accounts when the database is empty.</summary>
    public bool Enabled { get; init; }

    /// <summary>Password given to the demo accounts. Supply it from user-secrets or an environment variable.</summary>
    public string? DemoPassword { get; init; }

    /// <summary>Extra HR accounts with no password, e.g. your own Gmail address for Google sign-in.</summary>
    public string[] GoogleHrEmails { get; init; } = [];
}
