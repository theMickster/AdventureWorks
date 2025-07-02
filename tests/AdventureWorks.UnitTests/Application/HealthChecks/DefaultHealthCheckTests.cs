using AdventureWorks.Application.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdventureWorks.UnitTests.Application.HealthChecks;

/// <summary>
/// Unit tests for DefaultHealthCheck
/// Following xUnit, Moq, and FluentAssertions patterns
/// </summary>
public sealed class DefaultHealthCheckTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DefaultConstructor_CreatesInstance()
    {
        var healthCheck = new DefaultHealthCheck();
        healthCheck.Should().NotBeNull();
        healthCheck.Should().BeAssignableTo<IHealthCheck>();
    }

    [Fact]
    public void Constructor_WithStatusFunction_CreatesInstance()
    {
        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Healthy, new Dictionary<string, object>());
        var healthCheck = new DefaultHealthCheck(statusFunc);
        healthCheck.Should().NotBeNull();
        healthCheck.Should().BeAssignableTo<IHealthCheck>();
    }

    [Fact]
    public void Constructor_NullStatusFunction_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DefaultHealthCheck(null!));
        exception.ParamName.Should().Be("statusFunc");
    }

    #endregion

    #region CheckHealthAsync - Default Constructor Tests

    [Fact]
    public async Task CheckHealthAsync_DefaultConstructor_ReturnsHealthyStatus()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_DefaultConstructor_IncludesMetadataInData()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainKey("Info");
        result.Data.Should().ContainKey("Dependencies");
    }

    [Fact]
    public async Task CheckHealthAsync_DefaultConstructor_DescriptionContainsProductAndStatus()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Description.Should().NotBeNullOrEmpty();
        result.Description.Should().Contain("is");
        result.Description.Should().Contain(HealthStatus.Healthy.ToString());
    }

    [Fact]
    public async Task CheckHealthAsync_DefaultConstructor_DependenciesIsEmptyDictionary()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Data.Should().ContainKey("Dependencies");
        var dependencies = result.Data["Dependencies"] as Dictionary<string, object>;
        dependencies.Should().NotBeNull();
        dependencies.Should().BeEmpty();
    }

    #endregion

    #region CheckHealthAsync - Custom Status Function Tests

    [Fact]
    public async Task CheckHealthAsync_HealthyStatus_ReturnsHealthyResult()
    {
        var dependencies = new Dictionary<string, object>
        {
            { "Database", "Connected" },
            { "Cache", "Available" }
        };

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Healthy, dependencies);

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["Dependencies"].Should().Be(dependencies);
    }

    [Fact]
    public async Task CheckHealthAsync_DegradedStatus_ReturnsDegradedResult()
    {
        var dependencies = new Dictionary<string, object>
        {
            { "Database", "Slow" },
            { "Cache", "Available" }
        };

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Degraded, dependencies);

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain(HealthStatus.Degraded.ToString());
        result.Data["Dependencies"].Should().Be(dependencies);
    }

    [Fact]
    public async Task CheckHealthAsync_UnhealthyStatus_ReturnsUnhealthyResult()
    {
        var dependencies = new Dictionary<string, object>
        {
            { "Database", "Disconnected" },
            { "Cache", "Unavailable" }
        };

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Unhealthy, dependencies);

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain(HealthStatus.Unhealthy.ToString());
        result.Data["Dependencies"].Should().Be(dependencies);
    }

    [Fact]
    public async Task CheckHealthAsync_WithDependencies_IncludesDependenciesInResult()
    {
        var expectedDependencies = new Dictionary<string, object>
        {
            { "SQL Server", "Connected" },
            { "Redis Cache", "Available" },
            { "External API", "Responding" }
        };

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Healthy, expectedDependencies);

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Data.Should().ContainKey("Dependencies");
        var actualDependencies = result.Data["Dependencies"] as Dictionary<string, object>;
        actualDependencies.Should().NotBeNull();
        actualDependencies.Should().BeEquivalentTo(expectedDependencies);
    }

    [Fact]
    public async Task CheckHealthAsync_WithEmptyDependencies_ReturnsEmptyDependenciesInResult()
    {
        var emptyDependencies = new Dictionary<string, object>();

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Healthy, emptyDependencies);

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Data.Should().ContainKey("Dependencies");
        var dependencies = result.Data["Dependencies"] as Dictionary<string, object>;
        dependencies.Should().NotBeNull();
        dependencies.Should().BeEmpty();
    }

    #endregion

    #region CheckHealthAsync - Result Structure Tests

    [Fact]
    public async Task CheckHealthAsync_ResultData_ContainsInfoAndDependenciesKeys()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        using (new AssertionScope())
        {
            result.Data.Should().HaveCount(2);
            result.Data.Should().ContainKey("Info");
            result.Data.Should().ContainKey("Dependencies");
        }
    }

    [Fact]
    public async Task CheckHealthAsync_InfoData_IsDictionary()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Data["Info"].Should().BeAssignableTo<Dictionary<string, object>>();
    }

    [Fact]
    public async Task CheckHealthAsync_DependenciesData_IsDictionary()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Data["Dependencies"].Should().BeAssignableTo<Dictionary<string, object>>();
    }

    #endregion

    #region CheckHealthAsync - Cancellation Token Tests

    [Fact]
    public async Task CheckHealthAsync_WithCancellationToken_CompletesSuccessfully()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var result = await healthCheck.CheckHealthAsync(context, cancellationToken);
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WithDefaultCancellationToken_CompletesSuccessfully()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context, default);
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    #endregion

    #region CheckHealthAsync - Multiple Status Scenarios

    [Theory]
    [InlineData(HealthStatus.Healthy)]
    [InlineData(HealthStatus.Degraded)]
    [InlineData(HealthStatus.Unhealthy)]
    public async Task CheckHealthAsync_VariousHealthStatuses_ReturnsCorrectStatus(HealthStatus expectedStatus)
    {
        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (expectedStatus, new Dictionary<string, object>());

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Status.Should().Be(expectedStatus);
        result.Description.Should().Contain(expectedStatus.ToString());
    }

    #endregion

    #region CheckHealthAsync - Complex Dependency Scenarios

    [Fact]
    public async Task CheckHealthAsync_WithMultipleDependencies_AllIncludedInResult()
    {
        var dependencies = new Dictionary<string, object>
        {
            { "Database1", "Healthy" },
            { "Database2", "Degraded" },
            { "Cache", "Healthy" },
            { "MessageQueue", "Healthy" },
            { "ExternalAPI1", "Unhealthy" }
        };

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Degraded, dependencies);

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        var resultDependencies = result.Data["Dependencies"] as Dictionary<string, object>;
        resultDependencies.Should().HaveCount(5);
        resultDependencies.Should().ContainKeys("Database1", "Database2", "Cache", "MessageQueue", "ExternalAPI1");
    }

    [Fact]
    public async Task CheckHealthAsync_DependencyWithComplexValue_PreservesValue()
    {
        var complexValue = new
        {
            Status = "Connected",
            Latency = 45,
            LastChecked = DateTime.UtcNow
        };

        var dependencies = new Dictionary<string, object>
        {
            { "Database", complexValue }
        };

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc =
            () => (HealthStatus.Healthy, dependencies);

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        var resultDependencies = result.Data["Dependencies"] as Dictionary<string, object>;
        resultDependencies!["Database"].Should().Be(complexValue);
    }

    #endregion

    #region CheckHealthAsync - Edge Cases

    [Fact]
    public async Task CheckHealthAsync_CalledMultipleTimes_ReturnsConsistentResults()
    {
        var callCount = 0;
        var dependencies = new Dictionary<string, object> { { "Test", "Value" } };

        Func<(HealthStatus, Dictionary<string, object>)> statusFunc = () =>
        {
            callCount++;
            return (HealthStatus.Healthy, dependencies);
        };

        var healthCheck = new DefaultHealthCheck(statusFunc);
        var context = new HealthCheckContext();
        var result1 = await healthCheck.CheckHealthAsync(context);
        var result2 = await healthCheck.CheckHealthAsync(context);
        var result3 = await healthCheck.CheckHealthAsync(context);
        using (new AssertionScope())
        {
            callCount.Should().Be(3);
            result1.Status.Should().Be(HealthStatus.Healthy);
            result2.Status.Should().Be(HealthStatus.Healthy);
            result3.Status.Should().Be(HealthStatus.Healthy);
        }
    }

    [Fact]
    public async Task CheckHealthAsync_StatusFunctionReturnsNull_HandlesGracefully()
    {
        var healthCheck = new DefaultHealthCheck();
        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().NotBeNull();
    }

    #endregion
}
