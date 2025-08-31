using AdventureWorks.DbReset.Console.Safety;
using AdventureWorks.DbReset.Console.Snapshot.Internal;
using AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Safety;

public sealed class SqlSourceMarkerProbeTests
{
    private const string TargetCs = "Server=localhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;";

    private static SqlSourceMarkerProbe BuildProbe(Mock<ISqlScriptExecutor> executor) => new(executor.Object);

    [Fact]
    public void HasMarker_ScalarReturnsOne_ReturnsTrue()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                TargetCs,
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)1);

        var probe = BuildProbe(executor);

        var result = probe.HasMarker(TargetCs, "dbreset.role", "source");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasMarker_ScalarReturnsNull_ReturnsFalse()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);

        var probe = BuildProbe(executor);

        probe.HasMarker(TargetCs, "dbreset.role", "source").Should().BeFalse();
    }

    [Fact]
    public void HasMarker_ScalarReturnsDBNull_ReturnsFalse()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(DBNull.Value);

        var probe = BuildProbe(executor);

        probe.HasMarker(TargetCs, "dbreset.role", "source").Should().BeFalse();
    }

    [Fact]
    public void HasMarker_ExecutorThrows_FailClosed_PropagatesException()
    {
        // Note: SqlException has no public constructor. The fail-closed semantics — never swallow,
        // always propagate — are verified here with a sentinel exception. End-to-end SqlException
        // coverage lands in the integration tests under #927/#928.
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("simulated SqlException"));

        var probe = BuildProbe(executor);

        Action act = () => probe.HasMarker(TargetCs, "dbreset.role", "source");

        act.Should().Throw<InvalidOperationException>().WithMessage("simulated SqlException");
    }

    [Fact]
    public void HasMarker_IssuedSqlContainsParameterizedLookups()
    {
        string? capturedSql = null;
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, sql, _, _, _) => capturedSql = sql);

        var probe = BuildProbe(executor);

        probe.HasMarker(TargetCs, "dbreset.role", "source");

        capturedSql.Should().NotBeNull();
        capturedSql.Should().Contain("[name] = @property");
        capturedSql.Should().Contain("CAST([value] AS NVARCHAR(MAX)) = @value");
    }

    [Fact]
    public void HasMarker_PassesPropertyAndValueAsParameters()
    {
        IReadOnlyList<SqlParameter>? capturedParams = null;
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, _, parameters, _, _) => capturedParams = parameters);

        var probe = BuildProbe(executor);

        probe.HasMarker(TargetCs, "dbreset.role", "source");

        capturedParams.Should().NotBeNull();
        capturedParams.Should().HaveCount(2);
        capturedParams.Should().Contain(p => p.ParameterName == "@property" && (string)p.Value! == "dbreset.role");
        capturedParams.Should().Contain(p => p.ParameterName == "@value" && (string)p.Value! == "source");
    }

    [Theory]
    [InlineData(null, "p", "v")]
    [InlineData("", "p", "v")]
    [InlineData("   ", "p", "v")]
    [InlineData("cs", null, "v")]
    [InlineData("cs", "", "v")]
    [InlineData("cs", "   ", "v")]
    [InlineData("cs", "p", null)]
    [InlineData("cs", "p", "")]
    [InlineData("cs", "p", "   ")]
    public void HasMarker_NullOrWhiteSpaceArguments_Throw(string? cs, string? property, string? value)
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var probe = BuildProbe(executor);

        Action act = () => probe.HasMarker(cs!, property!, value!);

        act.Should().Throw<ArgumentException>();
    }

    // -------------------- Additional QA coverage --------------------

    [Fact]
    public void HasMarker_IssuedSqlScopesToDatabaseLevelExtendedProperty()
    {
        // class = 0 AND major_id = 0 AND minor_id = 0 is the database-level selector.
        // Without this clause the probe could match a column-level or schema-level extended
        // property with the same name — defeating Rule #5.
        string? capturedSql = null;
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, sql, _, _, _) => capturedSql = sql);

        var probe = BuildProbe(executor);

        probe.HasMarker(TargetCs, "dbreset.role", "source");

        capturedSql.Should().NotBeNull();
        capturedSql.Should().Contain("class = 0");
        capturedSql.Should().Contain("major_id = 0");
        capturedSql.Should().Contain("minor_id = 0");
    }

    [Fact]
    public void HasMarker_PropagatesNonSqlExceptionFromExecutor()
    {
        // Defensive: the catch (if any) must NOT swallow a non-SqlException. Fail-closed semantics
        // mean the validator gets the error, not a silent "no marker" answer.
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("query timed out"));

        var probe = BuildProbe(executor);

        Action act = () => probe.HasMarker(TargetCs, "dbreset.role", "source");

        act.Should().Throw<TimeoutException>().WithMessage("query timed out");
    }

    [Fact]
    public void HasMarker_SqlException4060_TargetDbDoesNotExist_ReturnsFalse()
    {
        // SQL error 4060 = "Cannot open database requested by the login." This is the first-run
        // scenario: the target DB has never been created on this instance. A non-existent DB
        // cannot carry the source marker, so HasMarker returns false and lets restore proceed.
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(4060, "Cannot open database 'AdventureWorks_E2E' requested by the login."));

        var probe = BuildProbe(executor);

        var result = probe.HasMarker(TargetCs, "dbreset.role", "source");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(18456)]  // Login failed
    [InlineData(53)]     // Network path not found
    [InlineData(4064)]   // Cannot open user default database
    public void HasMarker_OtherSqlException_FailClosed_Propagates(int errorNumber)
    {
        // Non-4060 SqlExceptions must still propagate — fail-closed semantics. We must not
        // silently return false for auth failures or network errors.
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(errorNumber, $"SQL error {errorNumber}"));

        var probe = BuildProbe(executor);

        Action act = () => probe.HasMarker(TargetCs, "dbreset.role", "source");

        act.Should().Throw<SqlException>();
    }
}
