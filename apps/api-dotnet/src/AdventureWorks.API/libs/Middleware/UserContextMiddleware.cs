using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.Interfaces;
using MediatR;
using System.Security.Claims;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs.Middleware;

/// <summary>
/// Middleware that resolves the authenticated user's Entra identity and maps to BusinessEntity.
/// Populates UserContext early in the pipeline for use by controllers and handlers.
/// Runs after authentication middleware but before authorization.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class UserContextMiddleware(
    RequestDelegate next, 
    ILogger<UserContextMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<UserContextMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Processes the HTTP request to resolve user context from JWT claims.
    /// Uses CQRS query to validate and retrieve linked Person entity.
    /// </summary>
    public async Task InvokeAsync(
        HttpContext context, 
        IUserContextAccessor userContextAccessor,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(userContextAccessor);
        ArgumentNullException.ThrowIfNull(mediator);

        var user = context.User;
        var entraObjectId = GetEntraObjectId(user);
        
        // Build initial context from JWT claims
        var userContext = new UserContext
        {
            IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
            EntraObjectId = entraObjectId,
            UserPrincipalName = GetUserPrincipalName(user),
            DisplayName = GetDisplayName(user)
        };

        // If authenticated with Entra ObjectId, resolve person via CQRS
        if (userContext.IsAuthenticated && entraObjectId.HasValue)
        {
            try
            {
                var query = new ReadEntraLinkedPersonQuery 
                { 
                    EntraObjectId = entraObjectId.Value 
                };
                
                var person = await mediator.Send(query, context.RequestAborted);

                if (person != null)
                {
                    userContext = userContext with
                    {
                        BusinessEntityId = person.BusinessEntityId,
                        PersonFullName = $"{person.FirstName} {person.LastName}".Trim()
                    };
                    
                    _logger.LogDebug(
                        "Resolved Entra user: {UserPrincipalName} -> BusinessEntityId {BusinessEntityId}",
                        userContext.UserPrincipalName,
                        person.BusinessEntityId);
                }
                else
                {
                    _logger.LogWarning(
                        "Entra user authenticated but not linked to AdventureWorks: EntraObjectId={EntraObjectId}, UPN={UserPrincipalName}",
                        entraObjectId,
                        userContext.UserPrincipalName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error resolving BusinessEntityId for EntraObjectId={EntraObjectId}", 
                    entraObjectId);
                // Continue without BusinessEntityId rather than failing the request
            }
        }
        else if (userContext.IsAuthenticated)
        {
            _logger.LogWarning("Authenticated user missing Entra ObjectId claim");
        }

        // Store context for use throughout the request
        userContextAccessor.SetUserContext(userContext);

        // Continue processing the request
        await _next(context);
    }

    /// <summary>
    /// Extracts the Entra Object ID (oid) claim from the user principal.
    /// Supports both full claim URI and short form.
    /// </summary>
    private static Guid? GetEntraObjectId(ClaimsPrincipal? user)
    {
        var oidClaim = user?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? user?.FindFirst("oid")?.Value;

        return Guid.TryParse(oidClaim, out var guid) ? guid : null;
    }

    /// <summary>
    /// Extracts the User Principal Name (upn) claim.
    /// </summary>
    private static string? GetUserPrincipalName(ClaimsPrincipal? user)
    {
        return user?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value
            ?? user?.FindFirst("preferred_username")?.Value;
    }

    /// <summary>
    /// Extracts the display name claim.
    /// </summary>
    private static string? GetDisplayName(ClaimsPrincipal? user)
    {
        return user?.FindFirst("name")?.Value
            ?? user?.FindFirst(ClaimTypes.Name)?.Value;
    }
}
