using AdventureWorks.Application.Interfaces;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs.Services;

/// <summary>
/// Provides thread-safe access to the current user context using HttpContext.
/// Follows the same pattern as CorrelationIdAccessor.
/// </summary>
internal sealed class UserContextAccessor(IHttpContextAccessor httpContextAccessor) : IUserContextAccessor
{
    private const string UserContextItemKey = "UserContext";
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private UserContext? CurrentContext
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue(UserContextItemKey, out var context) == true)
            {
                return context as UserContext;
            }
            return null;
        }
    }

    public Guid? EntraObjectId => CurrentContext?.EntraObjectId;
    
    public string? UserPrincipalName => CurrentContext?.UserPrincipalName;
    
    public string? DisplayName => CurrentContext?.DisplayName;
    
    public int? BusinessEntityId => CurrentContext?.BusinessEntityId;
    
    public string? PersonFullName => CurrentContext?.PersonFullName;
    
    public bool IsAuthenticated => CurrentContext?.IsAuthenticated ?? false;
    
    public bool IsLinkedToAdventureWorks => CurrentContext?.BusinessEntityId.HasValue ?? false;

    /// <summary>
    /// Sets the user context in HttpContext.Items (called by middleware).
    /// </summary>
    public void SetUserContext(UserContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items[UserContextItemKey] = context;
        }
    }
}
