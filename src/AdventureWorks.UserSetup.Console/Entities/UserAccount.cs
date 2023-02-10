namespace AdventureWorks.UserSetup.Console.Entities;

internal sealed class UserAccount
{
    public int Id { get; set; }

    public DateTime ModifiedDate { get; set; } 

    public string UserName { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
}
