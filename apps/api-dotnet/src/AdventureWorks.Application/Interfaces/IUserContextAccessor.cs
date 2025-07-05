namespace AdventureWorks.Application.Interfaces;

/// <summary>
/// Provides thread-safe access to the current authenticated user's context.
/// Populated by UserContextMiddleware early in the request pipeline.
/// </summary>
public interface IUserContextAccessor
{
    /// <summary>
    /// Gets the Microsoft Entra Object ID (oid claim) of the authenticated user.
    /// This GUID maps to BusinessEntity.Rowguid when IsEntraUser=true.
    /// </summary>
    Guid? EntraObjectId { get; }
    
    /// <summary>
    /// Gets the User Principal Name (upn/preferred_username claim).
    /// Typically in email format (user@domain.com).
    /// </summary>
    string? UserPrincipalName { get; }
    
    /// <summary>
    /// Gets the display name of the authenticated user (name claim).
    /// </summary>
    string? DisplayName { get; }
    
    /// <summary>
    /// Gets the BusinessEntityId if the user is mapped to AdventureWorks.
    /// Null if not mapped or authentication failed.
    /// </summary>
    int? BusinessEntityId { get; }
    
    /// <summary>
    /// Gets the full name of the person from AdventureWorks data.
    /// </summary>
    string? PersonFullName { get; }
    
    /// <summary>
    /// Indicates whether the user is authenticated via Entra.
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Indicates whether the authenticated user is linked to an AdventureWorks BusinessEntity.
    /// </summary>
    bool IsLinkedToAdventureWorks { get; }
    
    /// <summary>
    /// Sets the user context. Called by middleware only.
    /// </summary>
    /// <param name="context">The user context to set</param>
    void SetUserContext(UserContext context);
}