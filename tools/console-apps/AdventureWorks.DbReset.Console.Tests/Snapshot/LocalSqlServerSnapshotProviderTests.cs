using System.Data;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot;
using AdventureWorks.DbReset.Console.Snapshot.Internal;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Snapshot;

public sealed class LocalSqlServerSnapshotProviderTests
{
    private const string TargetName = "AdventureWorks_E2E";
    private const string TargetDbName = "AdventureWorks_E2E";
    private const string TargetCs = "Server=localhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;";
    private const string TargetPattern = "^AdventureWorks_(E2E|Test)$";

    private static IConfiguration BuildConfiguration(IDictionary<string, string?>? extras = null)
    {
        var data = new Dictionary<string, string?>
        {
            [$"ConnectionStrings:{TargetName}"] = TargetCs,
        };
        if (extras is not null)
        {
            foreach (var kvp in extras)
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    private static DbResetOptions BuildOptions() => new()
    {
        SnapshotSource = "AdventureWorksDev",
        DefaultTarget = TargetName,
        BaselinePath = "/baselines/AdventureWorks_baseline.bak",
        TargetNamePattern = TargetPattern,
        DbUpProjectPath = "database/dbup/AdventureWorks.DbUp",
        SourceMarker = new SourceMarkerOptions { Property = "dbreset.role", Value = "source" },
    };

    private static LocalSqlServerSnapshotProvider BuildProvider(
        Mock<ISqlScriptExecutor> executor,
        IConfiguration? configuration = null,
        DbResetOptions? options = null)
    {
        return new LocalSqlServerSnapshotProvider(
            options ?? BuildOptions(),
            configuration ?? BuildConfiguration(),
            executor.Object,
            NullLogger<LocalSqlServerSnapshotProvider>.Instance);
    }

    /// <summary>
    /// Returns a fake <see cref="IDataReader"/> backed by a single row of (DataPath, LogPath).
    /// Used by the SERVERPROPERTY query in <c>ReadTargetFileSystemAsync</c>.
    /// </summary>
    private static IDataReader BuildServerPropertyReader(string? dataPath, string? logPath)
    {
        var dt = new DataTable();
        dt.Columns.Add("DataPath", typeof(string));
        dt.Columns.Add("LogPath", typeof(string));
        dt.Rows.Add((object?)dataPath ?? DBNull.Value, (object?)logPath ?? DBNull.Value);
        return dt.CreateDataReader();
    }

    private static IDataReader BuildFileListReader()
    {
        var dt = new DataTable();
        dt.Columns.Add("LogicalName", typeof(string));
        dt.Columns.Add("PhysicalName", typeof(string));
        dt.Columns.Add("Type", typeof(string));
        dt.Columns.Add("Size", typeof(long));
        dt.Rows.Add("AdventureWorks", "/baselines/AdventureWorks.mdf", "D", 100L);
        dt.Rows.Add("AdventureWorks_log", "/baselines/AdventureWorks_log.ldf", "L", 50L);
        return dt.CreateDataReader();
    }

    private static IDataReader BuildStateUserAccessReader(int state, int userAccess)
    {
        var dt = new DataTable();
        dt.Columns.Add("state", typeof(int));
        dt.Columns.Add("user_access", typeof(int));
        dt.Rows.Add(state, userAccess);
        return dt.CreateDataReader();
    }

    /// <summary>
    /// Stages mock returns for the happy-path RESTORE flow with a given starting state.
    /// </summary>
    private static List<string> StageRestoreHappyPath(Mock<ISqlScriptExecutor> executor, int initialState)
    {
        var invocationOrder = new List<string>();

        // OpenAsync — open primary connection on master.
        executor
            .Setup(e => e.OpenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new SqlConnection())
            .Callback<string, CancellationToken>((_, _) => invocationOrder.Add("Open"));

        // SERVERPROPERTY query (ReadOnConnection with a tuple projector).
        executor
            .Setup(e => e.ReadOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("SERVERPROPERTY")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Func<IDataReader, (string?, string?)>>()))
            .ReturnsAsync((SqlConnection _, string _, IReadOnlyList<SqlParameter> _, int _, CancellationToken _,
                Func<IDataReader, (string?, string?)> projector) =>
            {
                invocationOrder.Add("ServerProperty");
                using var reader = BuildServerPropertyReader("/var/opt/mssql/data", "/var/opt/mssql/data");
                return projector(reader);
            });

        // sys.databases.state read (ScalarOnConnection).
        executor
            .Setup(e => e.ScalarOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("sys.databases") && s.Contains("state")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                invocationOrder.Add("ReadState");
                return (object)initialState;
            });

        // FILELISTONLY (ReadOnConnection with BackupFileListReader projector).
        executor
            .Setup(e => e.ReadOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("RESTORE FILELISTONLY")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Func<IDataReader, IReadOnlyList<BackupFileMetadata>>>()))
            .ReturnsAsync((SqlConnection _, string _, IReadOnlyList<SqlParameter> _, int _, CancellationToken _,
                Func<IDataReader, IReadOnlyList<BackupFileMetadata>> projector) =>
            {
                invocationOrder.Add("FileList");
                using var reader = BuildFileListReader();
                return projector(reader);
            });

        // ExecuteOnConnection — captures every ALTER / RESTORE on the primary connection.
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<SqlConnection, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, sql, _, _, _) => invocationOrder.Add(LabelOfSql(sql)));

        // ExecuteAsync (open-its-own-connection) — used by drop-marker step.
        executor
            .Setup(e => e.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, sql, _, _, _) => invocationOrder.Add($"Execute:{LabelOfSql(sql)}"));

        return invocationOrder;
    }

    private static string LabelOfSql(string sql)
    {
        if (sql.Contains("RESTORE DATABASE") && sql.Contains("WITH RECOVERY"))
        {
            return "RestoreWithRecovery";
        }
        if (sql.Contains("RESTORE DATABASE"))
        {
            return "RestoreFromDisk";
        }
        if (sql.Contains("SET SINGLE_USER"))
        {
            return "SingleUser";
        }
        if (sql.Contains("SET MULTI_USER"))
        {
            return "MultiUser";
        }
        if (sql.Contains("SET RECOVERY SIMPLE"))
        {
            return "RecoverySimple";
        }
        if (sql.Contains("sp_dropextendedproperty"))
        {
            return "DropMarker";
        }
        if (sql.Contains("sp_addextendedproperty") || sql.Contains("sp_updateextendedproperty"))
        {
            return "StampMarker";
        }
        if (sql.Contains("xp_create_subdir"))
        {
            return "CreateSubdir";
        }
        return "Other";
    }

    [Fact]
    public async Task RestoreSnapshotAsync_StateOnline_SkipsRecoveryPreStep()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var order = StageRestoreHappyPath(executor, initialState: 0);
        var sut = BuildProvider(executor);

        await sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        order.Should().NotContain("RestoreWithRecovery");
        // The first ExecuteOnConnection call is the SINGLE_USER ALTER.
        order.First(o => o is "RestoreWithRecovery" or "SingleUser").Should().Be("SingleUser");
    }

    [Fact]
    public async Task RestoreSnapshotAsync_StateRestoring_TriggersRecoveryPreStepBeforeSingleUser()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var order = StageRestoreHappyPath(executor, initialState: 1);
        var sut = BuildProvider(executor);

        await sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        var recoveryIdx = order.IndexOf("RestoreWithRecovery");
        var singleUserIdx = order.IndexOf("SingleUser");
        recoveryIdx.Should().BeGreaterThan(-1);
        singleUserIdx.Should().BeGreaterThan(-1);
        recoveryIdx.Should().BeLessThan(singleUserIdx);
    }

    [Fact]
    public async Task RestoreSnapshotAsync_HappyPath_OrdersStepsCorrectly()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var order = StageRestoreHappyPath(executor, initialState: 0);
        var sut = BuildProvider(executor);

        await sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        // Critical-path order: ServerProperty → ReadState → SingleUser → FileList → RestoreFromDisk
        // → MultiUser → RecoverySimple → Execute:DropMarker
        var critical = order.Where(o => o is "ServerProperty" or "ReadState" or "SingleUser"
            or "FileList" or "RestoreFromDisk" or "MultiUser" or "RecoverySimple" or "Execute:DropMarker").ToList();

        critical.Should().Equal(
            "ServerProperty",
            "ReadState",
            "SingleUser",
            "FileList",
            "RestoreFromDisk",
            "MultiUser",
            "RecoverySimple",
            "Execute:DropMarker");
    }

    [Fact]
    public async Task RestoreSnapshotAsync_DropMarkerUsesTargetDbConnectionString()
    {
        string? capturedCs = null;
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        StageRestoreHappyPath(executor, initialState: 0);
        executor
            .Setup(e => e.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("sp_dropextendedproperty")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (cs, _, _, _, _) => capturedCs = cs);

        var sut = BuildProvider(executor);

        await sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        capturedCs.Should().NotBeNull();
        var csb = new SqlConnectionStringBuilder(capturedCs!);
        csb.InitialCatalog.Should().Be(TargetDbName);
    }

    [Fact]
    public async Task RestoreSnapshotAsync_RestoreFails_FinallyMultiUserCleanupFires()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        StageRestoreHappyPath(executor, initialState: 0);

        // Override RESTORE step to throw.
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("RESTORE DATABASE") && !s.Contains("WITH RECOVERY") && s.Contains("FROM DISK")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("simulated restore failure"));

        var multiUserCallsWithNoneToken = 0;
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("SET MULTI_USER")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)))
            .Returns(Task.CompletedTask)
            .Callback(() => multiUserCallsWithNoneToken++);

        var sut = BuildProvider(executor);

        Func<Task> act = () => sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("simulated restore failure");
        multiUserCallsWithNoneToken.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RestoreSnapshotAsync_CancellationDuringRestore_PropagatesCancellationAndCleansUp()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        StageRestoreHappyPath(executor, initialState: 0);

        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("RESTORE DATABASE") && !s.Contains("WITH RECOVERY") && s.Contains("FROM DISK")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("cancelled mid-restore"));

        var multiUserCleanupInvoked = false;
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("SET MULTI_USER")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)))
            .Returns(Task.CompletedTask)
            .Callback(() => multiUserCleanupInvoked = true);

        using var cts = new CancellationTokenSource();
        var sut = BuildProvider(executor);

        Func<Task> act = () => sut.RestoreSnapshotAsync(TargetName, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>().WithMessage("cancelled mid-restore");
        multiUserCleanupInvoked.Should().BeTrue();
    }

    // -------------------- BaselineExistsAsync --------------------

    [Fact]
    public async Task BaselineExistsAsync_HappyPath_SumsFileSizesIntoPresent()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ReadAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("RESTORE FILELISTONLY")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Func<IDataReader, IReadOnlyList<BackupFileMetadata>>>()))
            .ReturnsAsync((string _, string _, IReadOnlyList<SqlParameter> _, int _, CancellationToken _,
                Func<IDataReader, IReadOnlyList<BackupFileMetadata>> projector) =>
            {
                using var reader = BuildFileListReader();
                return projector(reader);
            });

        var sut = BuildProvider(executor);

        var result = await sut.BaselineExistsAsync(TargetName, CancellationToken.None);

        result.Exists.Should().BeTrue();
        result.SizeBytes.Should().Be(150L); // 100 + 50
    }

    [Fact]
    public async Task BaselineExistsAsync_InvalidTargetName_ThrowsBeforeSql()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var sut = BuildProvider(executor);

        Func<Task> act = () => sut.BaselineExistsAsync("BadTargetName", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        executor.VerifyNoOtherCalls();
    }

    // -------------------- SelfHealStuckTargetAsync --------------------

    [Theory]
    [InlineData(0, 0, RecoveryAction.None)]
    [InlineData(1, 0, RecoveryAction.RecoveredFromRestoring)]
    [InlineData(0, 1, RecoveryAction.RestoredMultiUser)]
    [InlineData(1, 1, RecoveryAction.Both)]
    public async Task SelfHealStuckTargetAsync_MapsStateAndUserAccessToAction(
        int state, int userAccess, RecoveryAction expected)
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.OpenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new SqlConnection());

        executor
            .Setup(e => e.ReadOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("user_access")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Func<IDataReader, (int, int)>>()))
            .ReturnsAsync((SqlConnection _, string _, IReadOnlyList<SqlParameter> _, int _, CancellationToken _,
                Func<IDataReader, (int, int)> projector) =>
            {
                using var reader = BuildStateUserAccessReader(state, userAccess);
                return projector(reader);
            });

        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = BuildProvider(executor);

        var outcome = await sut.SelfHealStuckTargetAsync(TargetName, CancellationToken.None);

        outcome.Action.Should().Be(expected);
        outcome.StateChanged.Should().Be(expected != RecoveryAction.None);
    }

    // -------------------- CreateSnapshotAsync --------------------

    [Fact]
    public async Task CreateSnapshotAsync_ExpressEdition_OmitsCompressionToken()
    {
        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:AdventureWorksDev"] = TargetCs.Replace("AdventureWorks_E2E", "AdventureWorks", StringComparison.Ordinal),
            ["ConnectionStrings:AdventureWorksE2E"] = TargetCs,
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);

        // EngineEdition scalar = 4 (Express).
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("EngineEdition")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)4);

        var capturedSql = new List<string>();
        executor
            .Setup(e => e.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, sql, _, _, _) => capturedSql.Add(sql));

        var sut = BuildProvider(executor, configuration);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        var backupSql = capturedSql.Single(s => s.Contains("BACKUP DATABASE"));
        backupSql.Should().NotContain("COMPRESSION");
    }

    [Fact]
    public async Task CreateSnapshotAsync_NonExpressEdition_IncludesCompressionToken()
    {
        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:AdventureWorksDev"] = TargetCs.Replace("AdventureWorks_E2E", "AdventureWorks", StringComparison.Ordinal),
            ["ConnectionStrings:AdventureWorksE2E"] = TargetCs,
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);

        // EngineEdition scalar = 3 (Enterprise).
        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("EngineEdition")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)3);

        var capturedSql = new List<string>();
        executor
            .Setup(e => e.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, sql, _, _, _) => capturedSql.Add(sql));

        var sut = BuildProvider(executor, configuration);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        var backupSql = capturedSql.Single(s => s.Contains("BACKUP DATABASE"));
        backupSql.Should().Contain("COMPRESSION");
    }

    [Fact]
    public async Task CreateSnapshotAsync_QueriesEngineEditionExactlyOnce()
    {
        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:AdventureWorksDev"] = TargetCs.Replace("AdventureWorks_E2E", "AdventureWorks", StringComparison.Ordinal),
            ["ConnectionStrings:AdventureWorksE2E"] = TargetCs,
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);

        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("EngineEdition")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)3);
        executor
            .Setup(e => e.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = BuildProvider(executor, configuration);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        executor.Verify(
            e => e.ScalarAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("EngineEdition")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------- Additional QA coverage --------------------

    /// <summary>
    /// Helper for CreateSnapshot tests: captures every (connectionString, sql) emitted by
    /// <see cref="ISqlScriptExecutor.ExecuteAsync"/> in order. Returns the captured list and
    /// builds a configured provider with both Dev and E2E connection strings.
    /// </summary>
    private static (List<(string Cs, string Sql)> Captured, LocalSqlServerSnapshotProvider Sut, IConfiguration Config)
        BuildCreateSnapshotHarness(
            Mock<ISqlScriptExecutor> executor,
            int engineEdition,
            IDictionary<string, string?>? extras = null)
    {
        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:AdventureWorksDev"] = TargetCs.Replace("AdventureWorks_E2E", "AdventureWorks", StringComparison.Ordinal),
            ["ConnectionStrings:AdventureWorksE2E"] = TargetCs,
        };
        if (extras is not null)
        {
            foreach (var kvp in extras)
            {
                configData[kvp.Key] = kvp.Value;
            }
        }
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        executor
            .Setup(e => e.ScalarAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("EngineEdition")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)engineEdition);

        var captured = new List<(string Cs, string Sql)>();
        executor
            .Setup(e => e.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<string, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (cs, sql, _, _, _) => captured.Add((cs, sql)));

        var sut = BuildProvider(executor, configuration);
        return (captured, sut, configuration);
    }

    [Fact]
    public async Task RestoreSnapshotAsync_StateRestoring_FirstExecuteOnConnectionIsRecoveryWithRecovery()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        StageRestoreHappyPath(executor, initialState: 1);

        var executedSql = new List<string>();
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<SqlConnection, string, IReadOnlyList<SqlParameter>, int, CancellationToken>(
                (_, sql, _, _, _) => executedSql.Add(sql));

        var sut = BuildProvider(executor);

        await sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        // The very first ExecuteOnConnection in state=1 must be the RECOVERY pre-step.
        executedSql.Should().NotBeEmpty();
        executedSql[0].Should().Contain("RESTORE DATABASE").And.Contain("WITH RECOVERY");

        // It must precede any SET SINGLE_USER call.
        var firstSingleUserIdx = executedSql.FindIndex(s => s.Contains("SET SINGLE_USER", StringComparison.Ordinal));
        firstSingleUserIdx.Should().BeGreaterThan(0,
            "the SINGLE_USER lock must come AFTER the RECOVERY pre-step");
    }

    [Fact]
    public async Task RestoreSnapshotAsync_CancellationDuringSingleUserStep_RunsCleanupAndPropagatesOriginal()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        StageRestoreHappyPath(executor, initialState: 0);

        // Override the SINGLE_USER step (NOT the cleanup MULTI_USER) to throw OperationCanceledException.
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("SET SINGLE_USER")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("cancelled during SINGLE_USER"));

        var cleanupInvoked = false;
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("SET MULTI_USER")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)))
            .Returns(Task.CompletedTask)
            .Callback(() => cleanupInvoked = true);

        var sut = BuildProvider(executor);

        Func<Task> act = () => sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>()
            .WithMessage("cancelled during SINGLE_USER");
        cleanupInvoked.Should().BeTrue("the catch block must run MULTI_USER cleanup even when cancellation fires inside an inner step");
    }

    [Fact]
    public async Task RestoreSnapshotAsync_CleanupMultiUserItselfThrows_OriginalExceptionStillPropagates()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        StageRestoreHappyPath(executor, initialState: 0);

        // Override RESTORE to throw the "original" sentinel.
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("RESTORE DATABASE") && !s.Contains("WITH RECOVERY") && s.Contains("FROM DISK")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("ORIGINAL"));

        // Override every MULTI_USER call (success-path AND cleanup) to throw a different sentinel.
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("SET MULTI_USER")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("CLEANUP-FAILED"));

        var sut = BuildProvider(executor);

        Func<Task> act = () => sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        // Per AC: surfaces the original exception, not whatever the recovery attempt may throw.
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORIGINAL");
    }

    [Fact]
    public async Task RestoreSnapshotAsync_FileListEmpty_ThrowsInvalidOperationAndAttemptsCleanup()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        StageRestoreHappyPath(executor, initialState: 0);

        // Override FILELISTONLY to return zero rows.
        executor
            .Setup(e => e.ReadOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("RESTORE FILELISTONLY")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Func<IDataReader, IReadOnlyList<BackupFileMetadata>>>()))
            .ReturnsAsync((IReadOnlyList<BackupFileMetadata>)Array.Empty<BackupFileMetadata>());

        var cleanupInvoked = false;
        executor
            .Setup(e => e.ExecuteOnConnectionAsync(
                It.IsAny<SqlConnection>(),
                It.Is<string>(s => s.Contains("SET MULTI_USER")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)))
            .Returns(Task.CompletedTask)
            .Callback(() => cleanupInvoked = true);

        var sut = BuildProvider(executor);

        Func<Task> act = () => sut.RestoreSnapshotAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*FILELISTONLY*no rows*");
        cleanupInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSnapshotAsync_AzureSqlEdition_IncludesCompressionToken()
    {
        // EngineEdition 5 = Azure SQL Database. NOT Express; compression is supported.
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var (captured, sut, _) = BuildCreateSnapshotHarness(executor, engineEdition: 5);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        var backupSql = captured.Single(c => c.Sql.Contains("BACKUP DATABASE")).Sql;
        backupSql.Should().Contain("COMPRESSION");
    }

    [Fact]
    public async Task CreateSnapshotAsync_ExpressEditionWithExplicitCompressionTrue_OmitsCompressionAndDegrades()
    {
        // Express + user opt-in = degrade gracefully. AC: warning logged, compression omitted (no throw).
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var (captured, sut, _) = BuildCreateSnapshotHarness(
            executor,
            engineEdition: 4,
            extras: new Dictionary<string, string?> { ["DbReset:Backup:Compression"] = "true" });

        await sut.CreateSnapshotAsync(CancellationToken.None);

        var backupSql = captured.Single(c => c.Sql.Contains("BACKUP DATABASE")).Sql;
        backupSql.Should().NotContain("COMPRESSION");
    }

    [Fact]
    public async Task CreateSnapshotAsync_RunsXpCreateSubdirBeforeBackup()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var (captured, sut, _) = BuildCreateSnapshotHarness(executor, engineEdition: 3);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        var subdirIdx = captured.FindIndex(c => c.Sql.Contains("xp_create_subdir"));
        var backupIdx = captured.FindIndex(c => c.Sql.Contains("BACKUP DATABASE"));
        subdirIdx.Should().BeGreaterThanOrEqualTo(0, "xp_create_subdir must be issued");
        backupIdx.Should().BeGreaterThan(subdirIdx, "xp_create_subdir must run BEFORE BACKUP DATABASE");
    }

    [Fact]
    public async Task CreateSnapshotAsync_StampsSourceMarkerAfterBackup()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var (captured, sut, _) = BuildCreateSnapshotHarness(executor, engineEdition: 3);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        var backupIdx = captured.FindIndex(c => c.Sql.Contains("BACKUP DATABASE"));
        var stampIdx = captured.FindIndex(c =>
            c.Sql.Contains("sp_addextendedproperty") || c.Sql.Contains("sp_updateextendedproperty"));
        backupIdx.Should().BeGreaterThanOrEqualTo(0);
        stampIdx.Should().BeGreaterThan(backupIdx, "the source marker must be stamped AFTER the BACKUP completes");
    }

    [Fact]
    public async Task CreateSnapshotAsync_DoesNotIssueAnySingleUserOrAlterDatabaseOnSource()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var (captured, sut, _) = BuildCreateSnapshotHarness(executor, engineEdition: 3);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        captured.Should().NotContain(c => c.Sql.Contains("SINGLE_USER", StringComparison.Ordinal));
        captured.Should().NotContain(c => c.Sql.Contains("ALTER DATABASE", StringComparison.Ordinal));
        captured.Should().NotContain(c => c.Sql.Contains("RESTORE DATABASE", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CreateSnapshotAsync_StampsMarkerOnSourceDbContext()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        var (captured, sut, _) = BuildCreateSnapshotHarness(executor, engineEdition: 3);

        await sut.CreateSnapshotAsync(CancellationToken.None);

        var stamp = captured.Single(c =>
            c.Sql.Contains("sp_addextendedproperty") || c.Sql.Contains("sp_updateextendedproperty"));

        var csb = new SqlConnectionStringBuilder(stamp.Cs);
        // The SnapshotSource is "AdventureWorksDev" → its CS InitialCatalog is "AdventureWorks".
        csb.InitialCatalog.Should().Be("AdventureWorks");
        csb.InitialCatalog.Should().NotBe("master");
    }

    [Fact]
    public async Task BaselineExistsAsync_HappyPath_ReturnsConfiguredBaselinePath()
    {
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ReadAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("RESTORE FILELISTONLY")),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Func<IDataReader, IReadOnlyList<BackupFileMetadata>>>()))
            .ReturnsAsync((string _, string _, IReadOnlyList<SqlParameter> _, int _, CancellationToken _,
                Func<IDataReader, IReadOnlyList<BackupFileMetadata>> projector) =>
            {
                using var reader = BuildFileListReader();
                return projector(reader);
            });

        var sut = BuildProvider(executor);

        var result = await sut.BaselineExistsAsync(TargetName, CancellationToken.None);

        result.Path.Should().Be("/baselines/AdventureWorks_baseline.bak");
    }

    [Fact]
    public async Task BaselineExistsAsync_NonSqlExceptionFromExecutor_PropagatesNotMappedToMissing()
    {
        // Only SqlException with numbers 3201/3203/3241 maps to Missing. Everything else propagates.
        var executor = new Mock<ISqlScriptExecutor>(MockBehavior.Strict);
        executor
            .Setup(e => e.ReadAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<SqlParameter>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Func<IDataReader, IReadOnlyList<BackupFileMetadata>>>()))
            .ThrowsAsync(new InvalidOperationException("connectivity failure"));

        var sut = BuildProvider(executor);

        Func<Task> act = () => sut.BaselineExistsAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("connectivity failure");
    }
}
