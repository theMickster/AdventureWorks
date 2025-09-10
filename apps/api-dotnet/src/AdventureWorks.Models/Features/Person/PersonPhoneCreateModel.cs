namespace AdventureWorks.Models.Features.Person;

public sealed class PersonPhoneCreateModel
{
    public string PhoneNumber { get; set; } = string.Empty;

    public int PhoneNumberTypeId { get; set; }
}
