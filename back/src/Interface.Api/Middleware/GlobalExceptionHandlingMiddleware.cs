using FluentValidation;
using Interface.Application.Common.Exceptions;
using Interface.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Api.Middleware;

/// <summary>
/// The single place where exceptions become HTTP responses. Every error is written as RFC 7807
/// Problem Details, so controllers and handlers contain no try/catch blocks.
/// </summary>
public sealed class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var problem = MapToProblemDetails(exception);
        LogException(exception, problem.Status ?? StatusCodes.Status500InternalServerError);

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private ProblemDetails MapToProblemDetails(Exception exception) => exception switch
    {
        ValidationException validation => ValidationProblem(validation),
        NotFoundException => Problem(StatusCodes.Status404NotFound, "Resource not found", exception.Message),
        ConflictException conflict => Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message, conflict.Code),
        BusinessRuleViolationException rule =>
            Problem(StatusCodes.Status422UnprocessableEntity, "Business rule violated", exception.Message, rule.Code),
        AuthenticationFailedException => Problem(StatusCodes.Status401Unauthorized, "Authentication failed", exception.Message),
        ForbiddenAccessException => Problem(StatusCodes.Status403Forbidden, "Forbidden", exception.Message),
        BadHttpRequestException badRequest => Problem(badRequest.StatusCode, "Bad request", exception.Message),
        OperationCanceledException => Problem(499, "Request cancelled", "The client closed the request."),
        _ => Problem(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred",
            environment.IsDevelopment() ? exception.ToString() : "Please try again later or contact support.")
    };

    private static ProblemDetails Problem(int status, string title, string detail, string? code = null)
    {
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        if (code is not null)
            problem.Extensions["code"] = code;
        return problem;
    }

    // Same JSON shape as MVC's automatic 400 response. "errors" is an extension member because the
    // Problem Details writer serializes the base ProblemDetails type and would drop a subclass property.
    private static ProblemDetails ValidationProblem(ValidationException exception)
    {
        var problem = Problem(StatusCodes.Status400BadRequest, "One or more validation errors occurred.",
            "See the errors property for details.");
        problem.Extensions["errors"] = ValidationErrors.ToDictionary(exception);
        return problem;
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogInformation("Request failed with {StatusCode}: {Reason}", statusCode, exception.Message);
    }
}
