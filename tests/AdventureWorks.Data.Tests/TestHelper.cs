using Microsoft.Extensions.Configuration;

namespace AdventureWorks.Data.Tests
{
    public class TestHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange:true)
                .AddEnvironmentVariables()
                .Build();
        }
        
    }
}
