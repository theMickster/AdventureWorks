namespace AdventureWorks.UserSetup.Console.Entities;

internal sealed class UserAccount
{
    public int Id { get; set; }

    public DateTime ModifiedDate { get; set; } 

    public string UserName { get; set; }
    
    public string PasswordHash { get; set; }
}
