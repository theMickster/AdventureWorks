namespace AdventureWorks.Models.Features.Person;

public sealed class PersonPhoneModel
{
    public string PhoneNumber { get; set; } = string.Empty;

    public int PhoneNumberTypeId { get; set; }

    public string PhoneNumberTypeName { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
