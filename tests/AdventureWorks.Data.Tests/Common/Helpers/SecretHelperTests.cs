using AdventureWorks.Common.Helpers;
using AdventureWorks.UnitTests.Setup;

namespace AdventureWorks.UnitTests.Common.Helpers;

[ExcludeFromCodeCoverage]
public sealed class SecretHelperTests : UnitTestBase
{
    public SecretHelperTests()
    {
        var values = new Dictionary<string, string>
        {
            { "foo", "foo value goes here" },
            { "foo1", "foo1 value goes here" },
            { "foo2", "I pity the fool" }
        };

        SecretHelper.UseMockData(values);
    }

    [Fact]
    public void FakeMockedData_ShouldReturnMockResults_Succeeds()
    {
        var result = SecretHelper.GetSecret("foo");

        using (new AssertionScope())
        {
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("foo value goes here");
        }
    }

    [Fact]
    public async Task WhenKeyIsFound_GetSecretAsyncSucceedsAsync()
    {
        var result = await SecretHelper.GetSecretAsync("foo1").ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("foo1 value goes here");
        }
    }

    [Fact]
    public void WhenKeyIsFound_GetSecretSucceeds()
    {
        var result = SecretHelper.GetSecret("foo2");

        using (new AssertionScope())
        {
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("I pity the fool");
        }
    }

    [Fact]
    public async Task WhenKeyIsNotFound_GetSecretAsyncSucceedsAsync()
    {
        var result = await SecretHelper.GetSecretAsync("something-else-async-goes-here")
            .ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Should().BeNull();
        }
    }

    [Fact]
    public void WhenKeyIsNotFound_GetSecretSucceeds()
    {
        var result = SecretHelper.GetSecret("something-else-goes-here");

        using (new AssertionScope())
        {
            result.Should().BeNull();
        }
    }
}