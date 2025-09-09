namespace AdventureWorks.Models.Features.Person;

public sealed class PersonEmailModel
{
    public int EmailAddressId { get; set; }

    public string EmailAddress { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
