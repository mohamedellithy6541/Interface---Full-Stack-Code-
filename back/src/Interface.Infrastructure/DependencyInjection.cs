using Interface.Application.Abstractions.Authentication;
using Interface.Application.Abstractions.Payroll;
using Interface.Application.Abstractions.Persistence;
using Interface.Infrastructure.Authentication;
using Interface.Infrastructure.Payroll;
using Interface.Infrastructure.Persistence;
using Interface.Infrastructure.Persistence.Interceptors;
using Interface.Infrastructure.Persistence.Repositories;
using Interface.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Interface.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    /// <summary>Registers persistence, authentication services and the configured payroll policy.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddAuthenticationServices(configuration);
        services.AddPayrollPolicy(configuration);
        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
                               ?? throw new InvalidOperationException(
                                   $"Connection string '{ConnectionStringName}' is not configured.");

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
    }

    private static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();
    }

    private static void AddPayrollPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PayrollOptions>()
            .Bind(configuration.GetSection(PayrollOptions.SectionName))
            .Validate(IsValidPolicy, "The Payroll configuration section is invalid (check tax brackets and rates).")
            .ValidateOnStart();

        services.AddSingleton<IPayrollPolicyProvider, ConfigurationPayrollPolicyProvider>();
    }

    private static bool IsValidPolicy(PayrollOptions options)
    {
        try
        {
            options.ToPolicy();
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
