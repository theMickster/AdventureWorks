namespace AdventureWorks.Models.Features.Sales;

public sealed class SalesPersonUpdateModel : SalesPersonBaseModel
{
    public int Id { get; set; }

    // Person fields (updatable)
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Title { get; set; }
    public string? Suffix { get; set; }

    // Employee fields (updatable only - immutable fields excluded)
    public required string JobTitle { get; set; }
    public required string MaritalStatus { get; set; }
    public required string Gender { get; set; }
    public bool SalariedFlag { get; set; }
    public short? OrganizationLevel { get; set; }

    // Note: Immutable fields NOT included:
    // - NationalIdNumber (government ID cannot change)
    // - LoginId (system identity cannot change)
    // - BirthDate (cannot change birth date)
    // - HireDate (historical fact - original hire date)
}
