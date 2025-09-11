namespace AdventureWorks.Models.Features.Person;

/// <summary>
/// Consolidated detail model for a single person.
/// </summary>
public sealed class PersonDetailModel
{
    public int BusinessEntityId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string? Title { get; set; }

    public string? Suffix { get; set; }

    public string PersonTypeName { get; set; } = string.Empty;

    public int EmailPromotion { get; set; }

    public List<PersonEmailModel> EmailAddresses { get; set; } = [];

    public List<PersonPhoneModel> PhoneNumbers { get; set; } = [];
}
