using AdventureWorks.UserSetup.Console.DbContexts;
using AdventureWorks.UserSetup.Console.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddUserSecrets<Program>()
    .AddInMemoryCollection();

var configuration = builder.Build();

var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

Console.ForegroundColor = ConsoleColor.Cyan;

Console.WriteLine("Hello from the AdventureWorks User Setup Console App");

var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksUserSetupContext>();
optionsBuilder.UseSqlServer(defaultConnectionString);
var dbContext = new AdventureWorksUserSetupContext(optionsBuilder.Options);
var updateService = new UpdateUserAccountService(dbContext);
var password = "___";
/*
var success = await updateService.UpdateUserAccountPasswords(password).ConfigureAwait(false);

if (success)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Adventure Works User Account update was successful. Happy Panda");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Adventure Works User Account update was not successful. Sad Panda");
}
*/
password = "HellWorld";
Console.WriteLine(BC.HashPassword(password));

Console.ForegroundColor = ConsoleColor.White;

