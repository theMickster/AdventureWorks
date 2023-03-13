using AdventureWorks.Testing.Console;
using AdventureWorks.Testing.Console.libs;
using AdventureWorks.Testing.Console.Settings;
using AdventureWorks.Testing.Console.Verifications;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddUserSecrets<Program>()
    .AddInMemoryCollection();

var configuration = builder.Build();

DbConnectionString.ConnectionStringName = "DefaultConnection";
DbConnectionString.ConnectionString = configuration.GetConnectionString(DbConnectionString.ConnectionStringName) ?? string.Empty;

var serviceCollection = AutofacBootstrapper.BuildServiceCollection(configuration);

var containerBuilder = new ContainerBuilder();
containerBuilder.Populate(serviceCollection);

var serviceProvider = new AutofacServiceProvider(
    containerBuilder.BuildContainer(configuration)
        .Build());


Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Hello from the AdventureWorks Testing Console App");

//Parser.Default.ParseArguments<CommandLineOptions>(args)
//.WithParsed<CommandLineOptions>(
//    o =>
//    {
//        if (o.TestSecurityModel)
//        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("****************************************************************");
            Console.WriteLine("Verifying:: AdventureWorksDbContext ........ ");
            Console.WriteLine("****************************************************************");

            var (rtSuccess, errorList) = await new VerifyDbContext(serviceProvider).VerifyAllTheThings().ConfigureAwait(false);

//        }
//    }
//);

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("****************************************************************");
Console.WriteLine("Verifying:: AdventureWorksDbContext ........ ");
Console.WriteLine("****************************************************************");

var (storeSuccess, storeErrorList) = await new VerifyStoreRepository(serviceProvider).VerifyAllTheThings().ConfigureAwait(false);


Console.ForegroundColor = ConsoleColor.White;



