using AdventureWorks.Application.Exceptions;

namespace AdventureWorks.UnitTests.Application.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class ConfigurationExceptionTests : UnitTestBase
{
    [Fact]
    public void ErrorMessage_is_default()
    {
        _ = ((Action)MyConfigTesting.ThrowWithoutMessage)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.Message.Should().Contain("Exception of type 'AdventureWorks.Application.Exceptions.ConfigurationException'");

        _ = ((Action)MyConfigTesting.ThrowWithoutMessage)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.ErrorMessages.Count().Should().Be(0);

        _ = ((Action)MyConfigTesting.ThrowWithoutMessage)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.ToString().Should().StartWith("ConfigurationException(");

    }

    [Fact]
    public void ErrorMessage_is_correctly_captured()
    {
        _ = ((Action)MyConfigTesting.ThrowSimpleException)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.Message.Should().Contain("A simple configuration error.");
    }

    [Fact]
    public void ErrorMessages_are_correctly_captured()
    {
        _ = ((Action)MyConfigTesting.ThrowExceptionViaTwoParams)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.ErrorMessages.Count().Should().Be(3);

        _ = ((Action)MyConfigTesting.ThrowExceptionViaTwoParams)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.Message.Should().Contain("error01, error02, error03");
    }

    [Fact]
    public void InnerException_is_correctly_captured()
    {
        _ = ((Action)MyConfigTesting.ThrowExceptionWithInnerException)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .WithMessage("A configuration error has occurred")
            .WithInnerException<InvalidOperationException>()
            .WithMessage("An invalid operation has occurred");
    }

    [Fact]
    public void InnerException_and_Messages_are_correctly_captured()
    {
        _ = ((Action)MyConfigTesting.ThrowExceptionWithInnerExceptionAndErrorMessageList)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .WithMessage("A configuration error has occurred!!")
            .WithInnerException<InvalidOperationException>()
            .WithMessage("An invalid operation has occurred!!");

        _ = ((Action)MyConfigTesting.ThrowExceptionWithInnerExceptionAndErrorMessageList)
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.ErrorMessages.Count().Should().Be(2);
    }

    private static class MyConfigTesting
    {
        public static void ThrowWithoutMessage()
        {
            throw new ConfigurationException();
        }

        public static void ThrowSimpleException()
        {
            throw new ConfigurationException("A simple configuration error.");
        }

        public static void ThrowExceptionViaTwoParams()
        {
            throw new ConfigurationException(new List<string> { "error01", "error02", "error03" },
                "error01, error02, error03");
        }

        public static void ThrowExceptionWithInnerException()
        {
            throw new ConfigurationException(
                "A configuration error has occurred",
                        new InvalidOperationException("An invalid operation has occurred")
            );
        }


        public static void ThrowExceptionWithInnerExceptionAndErrorMessageList()
        {
            throw new ConfigurationException(
                new List<string> { "error01", "error02"},
                "A configuration error has occurred!!",
                new InvalidOperationException("An invalid operation has occurred!!")
            );
        }
    }
}