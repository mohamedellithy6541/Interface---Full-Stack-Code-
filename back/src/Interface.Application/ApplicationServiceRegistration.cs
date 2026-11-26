using FluentValidation;
using Interface.Application.Common.Behaviors;
using Interface.Domain.Payroll;
using Microsoft.Extensions.DependencyInjection;

namespace Interface.Application;

public static class ApplicationServiceRegistration
{
    /// <summary>Registers MediatR (CQRS handlers + pipeline), AutoMapper profiles, validators and domain services.</summary>
    /// <param name="autoMapperLicenseKey">Optional AutoMapper license key; without it AutoMapper only logs a warning.</param>
    public static IServiceCollection AddApplication(this IServiceCollection services, string? autoMapperLicenseKey = null)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(configuration =>
        {
            if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey))
                configuration.LicenseKey = autoMapperLicenseKey;
        }, assembly);

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITaxCalculator, ProgressiveTaxCalculator>();
        services.AddSingleton<ISocialInsuranceCalculator, SocialInsuranceCalculator>();
        services.AddSingleton<IPayrollCalculator, PayrollCalculator>();

        return services;
    }
}
