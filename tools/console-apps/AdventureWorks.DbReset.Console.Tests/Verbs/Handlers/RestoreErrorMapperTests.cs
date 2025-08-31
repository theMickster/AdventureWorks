using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class RestoreErrorMapperTests
{
    private const string TargetName = "AdventureWorks_E2E";
    private const string BaselinePath = "/baselines/AdventureWorks.bak";

    [Theory]
    [InlineData(4060)]   // cannot open database
    [InlineData(18456)]  // login failed
    [InlineData(53)]     // network path not found
    [InlineData(40)]     // could not open a connection
    [InlineData(10060)]  // tcp connect timeout
    [InlineData(10054)]  // existing connection forcibly closed
    public void TryMap_KnownNetworkErrors_ReturnTargetUnreachableExitCode(int sqlNumber)
    {
        var ex = SqlExceptionFactory.Create(sqlNumber, "boom");

        var result = RestoreErrorMapper.TryMap(ex, TargetName, BaselinePath);

        result.Should().NotBeNull();
        result!.ExitCode.Should().Be(DbResetDefaults.ExitRestoreTargetUnreachable);
        result.StdErr.Should().Contain(TargetName);
        result.StdErr.Should().NotContain("at Microsoft.Data.SqlClient");
        result.StdErr.Should().NotContain("\n");
    }

    [Fact]
    public void TryMap_NumberZeroWithInnerException_ReturnsTargetUnreachableExitCode()
    {
        var ex = SqlExceptionFactory.CreateWithInner(0, "transport-level failure");

        var result = RestoreErrorMapper.TryMap(ex, TargetName, BaselinePath);

        result.Should().NotBeNull();
        result!.ExitCode.Should().Be(DbResetDefaults.ExitRestoreTargetUnreachable);
        result.StdErr.Should().Contain(TargetName);
    }

    [Fact]
    public void TryMap_OperatingSystemError5_ReturnsPermissionDeniedExitCode()
    {
        var ex = SqlExceptionFactory.Create(
            3201,
            "Cannot open backup device '/baselines/AdventureWorks.bak'. Operating system error 5(Access is denied.).");

        var result = RestoreErrorMapper.TryMap(ex, TargetName, BaselinePath);

        result.Should().NotBeNull();
        result!.ExitCode.Should().Be(DbResetDefaults.ExitRestorePermissionDenied);
        result.StdErr.Should().Contain(BaselinePath);
        result.StdErr.Should().Contain("DOCKER.md");
        result.StdErr.Should().NotContain("at Microsoft.Data.SqlClient");
    }

    [Fact]
    public void TryMap_UnrecognizedSqlError_ReturnsNullSoCallerCanRethrow()
    {
        var ex = SqlExceptionFactory.Create(9999, "some other failure");

        var result = RestoreErrorMapper.TryMap(ex, TargetName, BaselinePath);

        result.Should().BeNull();
    }
}
