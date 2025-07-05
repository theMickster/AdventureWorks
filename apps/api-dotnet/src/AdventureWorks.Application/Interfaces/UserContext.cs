namespace AdventureWorks.Application.Interfaces;

/// <summary>
/// Represents the authenticated user's context for a request.
/// Immutable record to prevent accidental mutation.
/// </summary>
public sealed record UserContext
{
    public Guid? EntraObjectId { get; init; }

    public string? UserPrincipalName { get; init; }

    public string? DisplayName { get; init; }

    public int? BusinessEntityId { get; init; }

    public string? PersonFullName { get; init; }

    public bool IsAuthenticated { get; init; }
}