using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Common.Constants;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace AdventureWorks.API.libs.Middleware;

[ExcludeFromCodeCoverage]
public sealed class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Sanitized error message returned in the 500 response body when an unhandled exception type reaches the fall-through branch.
    /// Diagnostic detail is preserved in structured logs via LogError, not the response body.
    /// </summary>
    private const string GenericServerErrorMessage = "An unexpected error occurred.";

    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Invoke(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(correlationIdAccessor);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex, correlationIdAccessor);
        }
    }

    private Task HandleException(HttpContext context, Exception exception, ICorrelationIdAccessor correlationIdAccessor)
    {
        var httpStatusCode = HttpStatusCode.InternalServerError;
        var correlationId = correlationIdAccessor.CorrelationId ?? "N/A";

        // Log the exception with correlation ID
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method);

        context.Response.ContentType = "application/json";

        // Ensure correlation ID is in response headers
        if (!context.Response.Headers.ContainsKey(ConfigurationConstants.CorrelationIdHeaderName))
        {
            context.Response.Headers.Append(ConfigurationConstants.CorrelationIdHeaderName, correlationId);
        }

        var result = string.Empty;

        switch (exception)
        {
            case ValidationException validationException:
                httpStatusCode = HttpStatusCode.BadRequest;
                var errors = validationException.Errors.Select(x => new
                {
                    x.PropertyName,
                    x.ErrorCode,
                    x.ErrorMessage,
                    CorrelationId = correlationId
                });
                result = JsonSerializer.Serialize(errors, CamelCaseOptions);
                break;
            case KeyNotFoundException keyNotFoundException:
                httpStatusCode = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new
                {
                    error = keyNotFoundException.Message,
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow
                });
                break;
            case ConflictException conflictException:
                httpStatusCode = HttpStatusCode.Conflict;
                result = JsonSerializer.Serialize(new
                {
                    error = conflictException.Message,
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow
                });
                break;
        }

        context.Response.StatusCode = (int)httpStatusCode;

        if (result == string.Empty)
        {
            result = JsonSerializer.Serialize(new
            {
                error = GenericServerErrorMessage,
                correlationId = correlationId,
                timestamp = DateTime.UtcNow
            });
        }

        return context.Response.WriteAsync(result);
    }

}
