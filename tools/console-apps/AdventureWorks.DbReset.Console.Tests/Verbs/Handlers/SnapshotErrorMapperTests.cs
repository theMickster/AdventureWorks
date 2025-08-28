using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class SnapshotErrorMapperTests
{
    private const string SourceKey = "AdventureWorksDev";
    private const string BaselinePath = "/baselines/AdventureWorks.bak";

    [Theory]
    [InlineData(4060)]   // cannot open database
    [InlineData(18456)]  // login failed
    [InlineData(53)]     // network path not found
    [InlineData(40)]     // could not open a connection
    [InlineData(10060)]  // tcp connect timeout
    public void TryMap_KnownNetworkErrors_ReturnUnreachableExitCode(int sqlNumber)
    {
        var ex = SqlExceptionFactory.Create(sqlNumber, "boom");

        var result = SnapshotErrorMapper.TryMap(ex, SourceKey, BaselinePath);

        result.Should().NotBeNull();
        result!.ExitCode.Should().Be(DbResetDefaults.ExitSnapshotSourceUnreachable);
        result.StdErr.Should().Contain(SourceKey);
        result.StdErr.Should().NotContain("at Microsoft.Data.SqlClient");
        result.StdErr.Should().NotContain("\n");
    }

    [Fact]
    public void TryMap_OperatingSystemError5_ReturnsPermissionDeniedExitCode()
    {
        var ex = SqlExceptionFactory.Create(
            3201,
            "Cannot open backup device '/baselines/x.bak'. Operating system error 5(Access is denied.).");

        var result = SnapshotErrorMapper.TryMap(ex, SourceKey, BaselinePath);

        result.Should().NotBeNull();
        result!.ExitCode.Should().Be(DbResetDefaults.ExitSnapshotPermissionDenied);
        result.StdErr.Should().Contain(BaselinePath);
        result.StdErr.Should().Contain("DOCKER.md");
        result.StdErr.Should().NotContain("at Microsoft.Data.SqlClient");
    }

    [Fact]
    public void TryMap_OperatingSystemError5_TakesPrecedenceOverNetworkNumber()
    {
        // Some xp_create_subdir failures surface as 4060 with an OS-error-5 message — make sure
        // we route them as permission failures, not as "source unreachable".
        var ex = SqlExceptionFactory.Create(
            4060,
            "xp_create_subdir failed: Operating system error 5(Access is denied.).");

        var result = SnapshotErrorMapper.TryMap(ex, SourceKey, BaselinePath);

        result.Should().NotBeNull();
        result!.ExitCode.Should().Be(DbResetDefaults.ExitSnapshotPermissionDenied);
    }

    [Fact]
    public void TryMap_UnrecognizedSqlError_ReturnsNullSoCallerCanRethrow()
    {
        var ex = SqlExceptionFactory.Create(1234, "some other failure");

        var result = SnapshotErrorMapper.TryMap(ex, SourceKey, BaselinePath);

        result.Should().BeNull();
    }

    [Fact]
    public void TryMap_NullException_Throws()
    {
        var act = () => SnapshotErrorMapper.TryMap(null!, SourceKey, BaselinePath);

        act.Should().Throw<ArgumentNullException>();
    }
}
