using AdventureWorks.Common.Settings;

namespace AdventureWorks.UnitTests.Common.Settings;

[ExcludeFromCodeCoverage]
public sealed class DatabaseConnectionStringTests : UnitTestBase
{
    [Fact]
    public void ConnectionString_setter_throws_correct_exceptions()
    {
        var setting = new DatabaseConnectionString();

        using (new AssertionScope())
        {
            _ = ((Action)(() => setting.ConnectionString = null!))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.");

            _ = ((Action)(() => setting.ConnectionString = string.Empty))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.");

            _ = ((Action)(() => setting.ConnectionString = "     "))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.");

            _ = ((Action)(() => setting.ConnectionString = "hello world; servverr is misspelled"))
                .Should().Throw<InvalidOperationException>("because we expect a Invalid Operation exception.");

            _ = ((Action)(() => setting.ConnectionString = "hello world; server; databaseee is misspelled"))
                .Should().Throw<InvalidOperationException>("because we expect a Invalid Operation exception.");

            _ = ((Action)(() => setting.ConnectionString = "hello world; server;  database  initial catalg is misspelled"))
                .Should().Throw<InvalidOperationException>("because we expect a Invalid Operation exception.");

            _ = ((Action)(() => setting.ConnectionString = "hello world; server database;  initial catalog; app name is missing"))
                .Should().Throw<InvalidOperationException>("because we expect a Invalid Operation exception.");
        }
    }

    [Fact]
    public void ConnectionString_setter_succeeds()
    {
        var setting = new DatabaseConnectionString
        {
            ConnectionString = "Server=(local); Database=HelloWorld12345; Application Name=Test;",
            ConnectionStringName = "AWS"
        };

        setting.ConnectionString.Should().Be("Server=(local); Database=HelloWorld12345; Application Name=Test;");
        setting.ConnectionStringName.Should().Be("AWS");
    }

}
