namespace AdventureWorks.Models.Features.Person;

/// <summary>
/// Model representing a Person entity that is linked to a Microsoft Entra ID user.
/// Used for user context resolution and authentication mapping.
/// </summary>
public sealed record EntraLinkedPersonModel
{
    /// <summary>
    /// The BusinessEntityId of the person (primary key).
    /// </summary>
    public required int BusinessEntityId { get; init; }
    
    /// <summary>
    /// The Microsoft Entra Object ID (oid claim) from BusinessEntity.Rowguid.
    /// </summary>
    public required Guid EntraObjectId { get; init; }
    
    /// <summary>
    /// First name of the person.
    /// </summary>
    public required string FirstName { get; init; }
    
    /// <summary>
    /// Last name of the person.
    /// </summary>
    public required string LastName { get; init; }
    
    /// <summary>
    /// Middle name of the person (optional).
    /// </summary>
    public string? MiddleName { get; init; }
    
    /// <summary>
    /// Title (e.g., Mr., Ms., Dr.) (optional).
    /// </summary>
    public string? Title { get; init; }
    
    /// <summary>
    /// Primary email address of the person (optional).
    /// </summary>
    public string? EmailAddress { get; init; }
    
    /// <summary>
    /// PersonType identifier.
    /// </summary>
    public required int PersonTypeId { get; init; }
    
    /// <summary>
    /// PersonType name (e.g., Employee, Individual Customer).
    /// </summary>
    public required string PersonTypeName { get; init; }
    
    /// <summary>
    /// Indicates if this person is flagged as an Entra user.
    /// </summary>
    public required bool IsEntraUser { get; init; }
}
