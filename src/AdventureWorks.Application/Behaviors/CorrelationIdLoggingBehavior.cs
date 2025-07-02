using AdventureWorks.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs correlation IDs with every command/query execution.
/// This ensures distributed tracing context is propagated through the application layer.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public sealed class CorrelationIdLoggingBehavior<TRequest, TResponse>(
    ILogger<CorrelationIdLoggingBehavior<TRequest, TResponse>> logger,
    ICorrelationIdAccessor correlationIdAccessor) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<CorrelationIdLoggingBehavior<TRequest, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdAccessor _correlationIdAccessor = correlationIdAccessor ?? throw new ArgumentNullException(nameof(correlationIdAccessor));

    /// <summary>
    /// Handles the MediatR request by logging correlation ID context
    /// </summary>
    /// <param name="request">The request being processed</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the handler</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var correlationId = _correlationIdAccessor.CorrelationId;
        var requestName = typeof(TRequest).Name;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? "N/A",
            ["RequestName"] = requestName
        }))
        {
            _logger.LogInformation(
                "Processing {RequestName} with correlation ID: {CorrelationId}",
                requestName,
                correlationId ?? "N/A");

            try
            {
                var response = await next();

                _logger.LogInformation(
                    "Completed {RequestName} with correlation ID: {CorrelationId}",
                    requestName,
                    correlationId ?? "N/A");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing {RequestName} with correlation ID: {CorrelationId}. Exception: {ExceptionMessage}",
                    requestName,
                    correlationId ?? "N/A",
                    ex.Message);
                throw;
            }
        }
    }
}
