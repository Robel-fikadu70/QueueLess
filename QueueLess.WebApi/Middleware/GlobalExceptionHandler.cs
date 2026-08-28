using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Domain.Exceptions;

namespace QueueLess.WebApi.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        //default error patterns
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
                // Format FluentValidation error paths into a structured dictionary
                errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            case BusinessRuleException businessException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Business Rule Violation";
                detail = businessException.Message;
                break;
            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                title = "Unauthorized";
                detail = "You are not authorized to perform this operation.";
                break;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"http://httpstatuses.io/{statusCode}",
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        //If there are detailed validation error lists, attach'em
        if(errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}