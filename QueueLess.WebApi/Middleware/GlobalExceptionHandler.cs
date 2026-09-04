using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Domain.Exceptions;

namespace QueueLess.WebApi.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
     HttpContext httpContext,
     Exception exception,
     CancellationToken cancellationToken)
    {
        // REMOVED: The global _logger.LogError line from the top is gone.
        // This prevents logging stack traces for expected business/validation mistakes.

        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "Server Error";
        var detail = "An unexpected error occurred on the server.";
        object? errors = null;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Validation Error";
                detail = "One or more validation checks failed.";
                errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                // Logged as a clean, single-line warning (No stack trace)
                _logger.LogWarning("Input validation failed for path: {Path}", httpContext.Request.Path);
                break;

            case BusinessRuleException businessException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Business Rule Violation";
                detail = businessException.Message;

                // Logged as a clean, single-line warning (No stack trace)
                _logger.LogWarning("Business rule violated: {Message} on path: {Path}", businessException.Message, httpContext.Request.Path);
                break;

            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                title = "Unauthorized";
                detail = "You are not authorized to perform this operation.";

                _logger.LogWarning("Unauthorized access attempt on path: {Path}", httpContext.Request.Path);
                break;

            default:
                // Unhandled system exceptions (like database crashes) are logged as errors with full stack traces
                _logger.LogError(exception, "A critical unhandled server exception occurred on path: {Path}", httpContext.Request.Path);
                break;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}",
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}