using System.Text;
using System.Text.Json.Serialization;
using Interface.Api.Authentication;
using Interface.Api.Contracts;
using Interface.Application.Abstractions.Authentication;
using Interface.Domain.Users;
using Interface.Infrastructure.Authentication;
using Interface.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Interface.Api;

public static class PresentationServiceRegistration
{
    public const string CorsPolicyName = "Frontend";

    /// <summary>Registers everything the HTTP layer needs: controllers, Problem Details, auth, Swagger, CORS, health checks.</summary>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddAutoMapper(
            mapper => mapper.LicenseKey = configuration["AutoMapper:LicenseKey"],
            typeof(ApiMappingProfile).Assembly);

        services.AddJwtAuthentication(configuration);
        services.AddCorsForFrontend(configuration);
        services.AddSwaggerDocumentation();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

        return services;
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = AppClaimTypes.Name,
                    RoleClaimType = AppClaimTypes.Role
                };
            });

        services.AddAuthorizationBuilder()
            // Every endpoint requires a signed-in user unless it opts out with [AllowAnonymous].
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AuthorizationPolicies.HrOnly, policy => policy.RequireRole(UserRole.Hr));
    }

    private static void AddCorsForFrontend(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
    }

    private static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Interface Employee & Payroll API",
                Version = "v1",
                Description = "Employee management and monthly payroll calculation."
            });

            // Stable operation ids give the Angular client generator readable method names.
            options.CustomOperationIds(api => api.ActionDescriptor.RouteValues["action"]);
            options.SupportNonNullableReferenceTypes();
            options.NonNullableReferenceTypesAsRequired();

            var xmlDocumentation = Path.Combine(AppContext.BaseDirectory, "Interface.Api.xml");
            if (File.Exists(xmlDocumentation))
                options.IncludeXmlComments(xmlDocumentation);

            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the accessToken returned by /api/auth/login.",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            options.AddSecurityDefinition("Bearer", bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearerScheme] = [] });
        });
    }
}
