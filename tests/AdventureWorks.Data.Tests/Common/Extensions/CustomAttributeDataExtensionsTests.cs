using AdventureWorks.Common.Extensions;
using AdventureWorks.UnitTests.Setup;

namespace AdventureWorks.UnitTests.Common.Extensions;

[ExcludeFromCodeCoverage]
public sealed class CustomAttributeDataExtensionsTests : UnitTestBase
{
    [Fact]
    public void WhenAttributeIsPresent_ThenValueIsReturned()
    {
        var test = new TestClass();
        var systemUnderTest = test.GetType().GetCustomAttributesData();

        var result = systemUnderTest.First().TryParse(out var evalOutput);

        using (new AssertionScope())
        {
            result.Should().BeTrue();
            evalOutput.Should().Be("test value");
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    private sealed class UnitTestAttribute : Attribute
    {
#pragma warning disable IDE0060 // Needed for Test
        public UnitTestAttribute(string foo)
#pragma warning restore IDE0060 // Remove unused parameter
        {

        }
    }

    [UnitTest("test value")]
    private sealed class TestClass
    {

    }

}