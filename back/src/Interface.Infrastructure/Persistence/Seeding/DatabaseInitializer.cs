using Interface.Application.Abstractions.Authentication;
using Interface.Domain.Employees;
using Interface.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Interface.Infrastructure.Persistence.Seeding;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}

/// <summary>Applies pending EF Core migrations, then seeds demo data when seeding is enabled.</summary>
internal sealed class DatabaseInitializer(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<SeedOptions> seedOptions,
    TimeProvider timeProvider,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    public const string DemoHrEmail = "hr@interface.local";
    public const string DemoEmployeeEmail = "sara.ali@interface.local";

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var options = seedOptions.Value;
        if (!options.Enabled)
            return;

        await SeedEmployeesAndAccountsAsync(options, cancellationToken);
        await SeedGoogleHrAccountsAsync(options, cancellationToken);
    }

    private async Task SeedEmployeesAndAccountsAsync(SeedOptions options, CancellationToken cancellationToken)
    {
        if (await dbContext.Employees.AnyAsync(cancellationToken))
            return;

        if (string.IsNullOrWhiteSpace(options.DemoPassword))
        {
            logger.LogWarning("Seed:DemoPassword is not set; skipping demo data. See README for setup.");
            return;
        }

        var employees = DemoEmployees.Create(DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
        dbContext.Employees.AddRange(employees);

        var hrUser = User.Create(DemoHrEmail, "HR Administrator", UserRole.Hr);
        hrUser.SetPasswordHash(passwordHasher.Hash(hrUser, options.DemoPassword));

        var linkedEmployee = employees.Single(employee => employee.Email == DemoEmployeeEmail);
        var employeeUser = User.Create(linkedEmployee.Email, linkedEmployee.FullName, UserRole.Employee, linkedEmployee.Id);
        employeeUser.SetPasswordHash(passwordHasher.Hash(employeeUser, options.DemoPassword));

        dbContext.Users.AddRange(hrUser, employeeUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {EmployeeCount} demo employees and 2 demo accounts", employees.Count);
    }

    private async Task SeedGoogleHrAccountsAsync(SeedOptions options, CancellationToken cancellationToken)
    {
        foreach (var email in options.GoogleHrEmails.Where(email => !string.IsNullOrWhiteSpace(email)))
        {
            var normalizedEmail = Employee.NormalizeEmail(email);
            if (await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
                continue;

            dbContext.Users.Add(User.Create(normalizedEmail, normalizedEmail, UserRole.Hr));
            logger.LogInformation("Added Google HR account {Email}", normalizedEmail);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
