using AdventureWorks.Common.Settings;

namespace AdventureWorks.UnitTests.Common.Settings;

[ExcludeFromCodeCoverage]
public sealed class EntityFrameworkCoreSettingsTests : UnitTestBase
{
    [Fact]
    public void Property_gets_and_sets_succeeds()
    {
        EntityFrameworkCoreSettings.SettingsRootName.Should().Be("EntityFrameworkCoreSettings");
        var settings = new EntityFrameworkCoreSettings
        {
            CommandLogLevel = "Debug",
            CommandTimeout = 120,
            CurrentConnectionStringName = "Azure",
            DatabaseConnectionStrings = new List<DatabaseConnectionString> { new () }
        };

        using (new AssertionScope())
        {
            settings.CommandLogLevel.Should().Be("Debug");
            settings.CommandTimeout.Should().Be(120);
            settings.CurrentConnectionStringName.Should().Be("Azure");
            settings.DatabaseConnectionStrings.Count.Should().Be(1);
        }
    }
}
