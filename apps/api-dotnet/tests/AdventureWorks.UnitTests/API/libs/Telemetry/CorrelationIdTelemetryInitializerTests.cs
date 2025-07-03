using AdventureWorks.API.libs.Telemetry;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Common.Constants;
using Microsoft.ApplicationInsights.DataContracts;

namespace AdventureWorks.UnitTests.API.libs.Telemetry;

public sealed class CorrelationIdTelemetryInitializerTests
{
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;

    public CorrelationIdTelemetryInitializerTests()
    {
        _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();
    }

    [Fact]
    public void Constructor_NullCorrelationIdAccessor_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CorrelationIdTelemetryInitializer(null!));
        Assert.Equal("correlationIdAccessor", exception.ParamName);
    }

    [Fact]
    public void Initialize_NullTelemetry_ThrowsArgumentNullException()
    {
        var initializer = new CorrelationIdTelemetryInitializer(_correlationIdAccessorMock.Object);
        Assert.Throws<ArgumentNullException>(() => initializer.Initialize(null!));
    }

    [Fact]
    public void Initialize_WithCorrelationId_AddsToGlobalProperties()
    {
        var correlationId = "test-correlation-id-123";
        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns(correlationId);

        var telemetry = new RequestTelemetry();
        var initializer = new CorrelationIdTelemetryInitializer(_correlationIdAccessorMock.Object);
        initializer.Initialize(telemetry);
        Assert.Contains(ConfigurationConstants.CorrelationIdHeaderName, telemetry.Context.GlobalProperties.Keys);
        Assert.Equal(correlationId, telemetry.Context.GlobalProperties[ConfigurationConstants.CorrelationIdHeaderName]);

        Assert.Contains("CorrelationId", telemetry.Context.GlobalProperties.Keys);
        Assert.Equal(correlationId, telemetry.Context.GlobalProperties["CorrelationId"]);
    }

    [Fact]
    public void Initialize_NoCorrelationId_DoesNotAddProperties()
    {
        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns((string?)null);

        var telemetry = new RequestTelemetry();
        var initializer = new CorrelationIdTelemetryInitializer(_correlationIdAccessorMock.Object);
        initializer.Initialize(telemetry);
        Assert.DoesNotContain(ConfigurationConstants.CorrelationIdHeaderName, telemetry.Context.GlobalProperties.Keys);
        Assert.DoesNotContain("CorrelationId", telemetry.Context.GlobalProperties.Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Initialize_EmptyOrWhitespaceCorrelationId_DoesNotAddProperties(string correlationId)
    {
        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns(correlationId);

        var telemetry = new RequestTelemetry();
        var initializer = new CorrelationIdTelemetryInitializer(_correlationIdAccessorMock.Object);
        initializer.Initialize(telemetry);
        Assert.DoesNotContain(ConfigurationConstants.CorrelationIdHeaderName, telemetry.Context.GlobalProperties.Keys);
        Assert.DoesNotContain("CorrelationId", telemetry.Context.GlobalProperties.Keys);
    }

    [Fact]
    public void Initialize_ExistingPropertyWithSameKey_DoesNotOverwrite()
    {
        var correlationId = "test-correlation-id-123";
        var existingValue = "existing-value";
        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns(correlationId);

        var telemetry = new RequestTelemetry();
        telemetry.Context.GlobalProperties.Add(ConfigurationConstants.CorrelationIdHeaderName, existingValue);

        var initializer = new CorrelationIdTelemetryInitializer(_correlationIdAccessorMock.Object);
        initializer.Initialize(telemetry);
        Assert.Equal(existingValue, telemetry.Context.GlobalProperties[ConfigurationConstants.CorrelationIdHeaderName]);

        // But CorrelationId should still be added
        Assert.Contains("CorrelationId", telemetry.Context.GlobalProperties.Keys);
        Assert.Equal(correlationId, telemetry.Context.GlobalProperties["CorrelationId"]);
    }

    [Fact]
    public void Initialize_WorksWithDifferentTelemetryTypes()
    {
        var correlationId = "test-correlation-id-123";
        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns(correlationId);

        var requestTelemetry = new RequestTelemetry();
        var traceTelemetry = new TraceTelemetry();
        var eventTelemetry = new EventTelemetry();
        var dependencyTelemetry = new DependencyTelemetry();

        var initializer = new CorrelationIdTelemetryInitializer(_correlationIdAccessorMock.Object);
        initializer.Initialize(requestTelemetry);
        initializer.Initialize(traceTelemetry);
        initializer.Initialize(eventTelemetry);
        initializer.Initialize(dependencyTelemetry);
        Assert.Equal(correlationId, requestTelemetry.Context.GlobalProperties[ConfigurationConstants.CorrelationIdHeaderName]);
        Assert.Equal(correlationId, traceTelemetry.Context.GlobalProperties[ConfigurationConstants.CorrelationIdHeaderName]);
        Assert.Equal(correlationId, eventTelemetry.Context.GlobalProperties[ConfigurationConstants.CorrelationIdHeaderName]);
        Assert.Equal(correlationId, dependencyTelemetry.Context.GlobalProperties[ConfigurationConstants.CorrelationIdHeaderName]);
    }
}
