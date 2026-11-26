using System.Security.Cryptography;
using Interface.Api;
using Interface.Api.Middleware;
using Interface.Application;
using Interface.Infrastructure;
using Interface.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

UseEphemeralJwtKeyInDevelopment(builder);

builder.Services
    .AddApplication(builder.Configuration["AutoMapper:LicenseKey"])
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging(); // Outermost, so it logs the final status code written by the error middleware.
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseStatusCodePages(); // Empty 401/403/404 responses also get a Problem Details body.

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "Interface Payroll API");
}

app.UseCors(PresentationServiceRegistration.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions()).AllowAnonymous();

if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
}

await app.RunAsync();

// Lets the API start in Development without a committed secret. Tokens stop working after a restart;
// set Jwt:SigningKey with user-secrets or an environment variable to keep them.
static void UseEphemeralJwtKeyInDevelopment(WebApplicationBuilder builder)
{
    if (!builder.Environment.IsDevelopment() || !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]))
        return;

    builder.Configuration["Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    Console.WriteLine("Jwt:SigningKey is not set; using a random development key for this run.");
}

public partial class Program;
