using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>
/// Local/Docker SQL Server implementation of <see cref="IDatabaseSnapshotProvider"/>. Stateless
/// (registered as a singleton); each call opens its own non-pooled <see cref="SqlConnection"/>
/// for the destructive paths.
/// </summary>
/// <remarks>
/// <para>
/// <b>Cancellation contract for <see cref="RestoreSnapshotAsync"/>:</b> on any failure (including
/// <see cref="OperationCanceledException"/>) the catch block performs a best-effort
/// <c>SET MULTI_USER</c> recovery using <see cref="CancellationToken.None"/>, then rethrows the
/// original exception. The <c>finally</c> only disposes the primary connection — never throws.
/// </para>
/// <para>
/// <b>SERVERPROPERTY fallback:</b> if both <c>InstanceDefaultDataPath</c> and
/// <c>InstanceDefaultLogPath</c> return <c>NULL</c> (older SQL Server with no instance default
/// configured), the directories are inferred from the first <c>'D'</c> and first <c>'L'</c>
/// FILELISTONLY rows respectively.
/// </para>
/// </remarks>
internal sealed class LocalSqlServerSnapshotProvider : IDatabaseSnapshotProvider
{
    private const int DefaultCommandTimeoutSeconds = 60;
    private const int BaselineProbeTimeoutSeconds = 60;
    private const int RestoreCommandTimeoutSecondsDefault = 600;
    private const int BackupCommandTimeoutSecondsDefault = 1800;
    private const int EngineEditionExpress = 4;
    private const string ConfigKeyBackupCompression = "DbReset:Backup:Compression";
    private const string ConfigKeyRestoreTimeoutSeconds = "DbReset:RestoreTimeoutSeconds";
    private const string ConfigKeyBackupTimeoutSeconds = "DbReset:BackupTimeoutSeconds";

    // SQL error numbers that BaselineExistsAsync maps to BaselineStatus.Missing.
    private const int SqlErrorBackupFileNotFound = 3201;
    private const int SqlErrorBackupReadFailure = 3203;
    private const int SqlErrorNotAValidBackup = 3241;

    // sys.databases.state value: RESTORING. Online databases return 0 (the orchestrator only
    // branches on the RESTORING case, so 0 isn't named).
    private const int DbStateRestoring = 1;

    // sys.databases.user_access values we care about.
    private const int DbUserAccessSingleUser = 1;

    private readonly DbResetOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ISqlScriptExecutor _executor;
    private readonly ILogger<LocalSqlServerSnapshotProvider> _logger;
    private readonly Lazy<Regex> _targetNameRegex;

    public LocalSqlServerSnapshotProvider(
        DbResetOptions options,
        IConfiguration configuration,
        ISqlScriptExecutor executor,
        ILogger<LocalSqlServerSnapshotProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options;
        _configuration = configuration;
        _executor = executor;
        _logger = logger;
        _targetNameRegex = new Lazy<Regex>(
            () => new Regex(
                _options.TargetNamePattern,
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Pooled connection against <c>[master]</c> — no destructive state to recover, so reuse is
    /// safe and avoids per-call connection setup cost.
    /// </remarks>
    public async Task<BaselineStatus> BaselineExistsAsync(string targetName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        SqlIdentifier.ValidateAgainstPattern(targetName, _targetNameRegex.Value);

        var targetCs = ResolveConnectionString(targetName);
        var masterCs = BuildMasterCs(targetCs, pooling: true);
        var baselinePath = _options.BaselinePath;

        try
        {
            var files = await _executor.ReadAsync(
                masterCs,
                "RESTORE FILELISTONLY FROM DISK = @baselinePath",
                new[] { new SqlParameter("@baselinePath", baselinePath) },
                BaselineProbeTimeoutSeconds,
                ct,
                BackupFileListReader.Read);

            var totalSize = 0L;
            for (var i = 0; i < files.Count; i++)
            {
                totalSize += files[i].Size;
            }
            return BaselineStatus.Present(baselinePath, totalSize);
        }
        catch (SqlException ex) when (ex.Number is SqlErrorBackupFileNotFound
            or SqlErrorBackupReadFailure
            or SqlErrorNotAValidBackup)
        {
            _logger.LogInformation(
                "Baseline at {BaselinePath} not readable (SQL error {SqlErrorNumber}); reporting as Missing.",
                baselinePath,
                ex.Number);
            return BaselineStatus.Missing(baselinePath);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// <b>Express edition handling:</b> <c>BACKUP … COMPRESSION</c> is unsupported on
    /// <c>EngineEdition == 4</c> (Express). When detected, compression is forced off regardless of
    /// the <c>DbReset:Backup:Compression</c> setting; an explicit opt-in logs a warning rather
    /// than failing the verb.
    /// </para>
    /// <para>
    /// <b>Backup directory creation:</b> <c>xp_create_subdir</c> ensures the parent of
    /// <c>BaselinePath</c> exists before <c>BACKUP DATABASE</c>. Equivalent to a no-op when the
    /// directory already exists.
    /// </para>
    /// <para>
    /// <b>SourceMarker stamping:</b> the extended property is stamped on a <i>second</i>
    /// connection scoped to the source database (not <c>[master]</c>) — extended properties are
    /// per-DB and the call to <c>sp_addextendedproperty</c> requires the right DB context.
    /// </para>
    /// </remarks>
    public async Task<TimeSpan> CreateSnapshotAsync(CancellationToken ct)
    {
        var sourceName = _options.SnapshotSource;
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new InvalidOperationException("DbReset:SnapshotSource is not configured.");
        }

        var (sourceCs, sourceDbName) = ResolveTargetConnection(sourceName);

        var sourceDbQuoted = SqlIdentifier.Quote(sourceDbName);
        var sourceMasterCs = BuildMasterCs(sourceCs, pooling: true);

        var sw = Stopwatch.StartNew();

        // Edition check (Risk #3).
        var engineEditionRaw = await _executor.ScalarAsync(
            sourceMasterCs,
            "SELECT CAST(SERVERPROPERTY('EngineEdition') AS INT)",
            Array.Empty<SqlParameter>(),
            DefaultCommandTimeoutSeconds,
            ct);
        var engineEdition = engineEditionRaw is int i ? i : Convert.ToInt32(engineEditionRaw, CultureInfo.InvariantCulture);

        var configFlag = _configuration.GetValue<bool?>(ConfigKeyBackupCompression);
        bool useCompression;
        if (engineEdition == EngineEditionExpress)
        {
            useCompression = false;
            if (configFlag == true)
            {
                _logger.LogWarning(
                    "BACKUP COMPRESSION not supported on Express Edition; proceeding without it.");
            }
        }
        else
        {
            useCompression = configFlag ?? true;
        }

        var compressionToken = useCompression ? ", COMPRESSION" : string.Empty;
        var backupSql =
            $"BACKUP DATABASE {sourceDbQuoted} "
            + "TO DISK = @baselinePath "
            + $"WITH FORMAT, INIT{compressionToken}, NAME = @backupName";

        var backupName = string.Concat(
            "DbReset baseline of ",
            sourceDbName,
            " ",
            DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));

        // xp_create_subdir for the baseline directory (Risk #2 documented in #929).
        var backupDir = Path.GetDirectoryName(_options.BaselinePath);
        if (!string.IsNullOrEmpty(backupDir))
        {
            await _executor.ExecuteAsync(
                sourceMasterCs,
                "EXEC master.dbo.xp_create_subdir @directory",
                new[] { new SqlParameter("@directory", backupDir) },
                DefaultCommandTimeoutSeconds,
                ct);
        }

        var backupTimeout = _configuration.GetValue<int?>(ConfigKeyBackupTimeoutSeconds)
            ?? BackupCommandTimeoutSecondsDefault;

        await _executor.ExecuteAsync(
            sourceMasterCs,
            backupSql,
            new[]
            {
                new SqlParameter("@baselinePath", _options.BaselinePath),
                new SqlParameter("@backupName", backupName),
            },
            backupTimeout,
            ct);

        // Stamp the source marker on the source DB itself (Risk #1: extended property needs DB context).
        var sourceDbCs = BuildTargetDbCs(sourceCs, sourceDbName);
        await StampSourceMarkerAsync(sourceDbCs, ct);

        return sw.Elapsed;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// <b>Single non-pooled connection:</b> <c>SET SINGLE_USER … RESTORE … SET MULTI_USER</c> must
    /// run on one connection — pooling would let an idle pooled connection grab the single-user
    /// slot and block the verb. The connection is non-pooled so a faulted state isn't returned to
    /// the pool.
    /// </para>
    /// <para>
    /// <b>FILESTREAM directory uniqueness:</b> <see cref="MoveClauseBuilder"/> appends
    /// <see cref="DateTime.UtcNow"/>.<see cref="DateTime.Ticks"/> to FILESTREAM target
    /// directories. SQL Server requires the FILESTREAM container directory to <i>not</i> exist at
    /// <c>RESTORE</c> time; without ticks, repeat restores onto the same target would collide on
    /// the previous container path and fail with <c>5121</c>.
    /// </para>
    /// <para>
    /// <b>Failure recovery:</b> any exception inside the <c>try</c> triggers
    /// <see cref="TryCleanupMultiUserAsync"/> on a fresh non-pooled connection with
    /// <see cref="CancellationToken.None"/>; the original exception is then rethrown.
    /// </para>
    /// </remarks>
    public async Task<TimeSpan> RestoreSnapshotAsync(string targetName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        SqlIdentifier.ValidateAgainstPattern(targetName, _targetNameRegex.Value);

        var (targetCs, targetDbName) = ResolveTargetConnection(targetName);
        var targetDbQuoted = SqlIdentifier.Quote(targetDbName);
        var nonPooledMasterCs = BuildMasterCs(targetCs, pooling: false);
        var restoreTimeout = _configuration.GetValue<int?>(ConfigKeyRestoreTimeoutSeconds)
            ?? RestoreCommandTimeoutSecondsDefault;

        var sw = Stopwatch.StartNew();
        SqlConnection? connection = null;
        try
        {
            connection = await _executor.OpenAsync(nonPooledMasterCs, ct);

            var fs = await ReadTargetFileSystemAsync(connection, ct);
            await HealRestoringStateAsync(connection, targetDbQuoted, targetDbName, ct);
            await LockSingleUserAsync(connection, targetDbQuoted, ct);

            var files = await ReadBaselineFileListAsync(connection, ct);
            fs = ApplyFilelistFallbackIfNeeded(fs, files);

            await ExecuteRestoreAsync(connection, targetDbQuoted, targetDbName, fs, files, restoreTimeout, ct);
            await RestoreUserAccessAsync(connection, targetDbQuoted, ct);

            // Drop the source marker on a SECOND connection in target DB context (Risk #1).
            var targetDbCs = BuildTargetDbCs(targetCs, targetDbName);
            await DropSourceMarkerAsync(targetDbCs, ct);
        }
        catch
        {
            await TryCleanupMultiUserAsync(nonPooledMasterCs, targetDbQuoted, targetName);
            throw;
        }
        finally
        {
            if (connection is not null)
            {
                await connection.DisposeAsync();
            }
        }

        return sw.Elapsed;
    }

    private async Task HealRestoringStateAsync(
        SqlConnection connection,
        string targetDbQuoted,
        string targetDbName,
        CancellationToken ct)
    {
        var state = await ReadDatabaseStateAsync(connection, targetDbName, ct);
        if (state == DbStateRestoring)
        {
            await ExecAlterAsync(connection, $"RESTORE DATABASE {targetDbQuoted} WITH RECOVERY", ct);
        }
    }

    private Task LockSingleUserAsync(SqlConnection connection, string targetDbQuoted, CancellationToken ct)
        => ExecAlterAsync(
            connection,
            $"ALTER DATABASE {targetDbQuoted} SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
            ct);

    private async Task<IReadOnlyList<BackupFileMetadata>> ReadBaselineFileListAsync(
        SqlConnection connection,
        CancellationToken ct)
    {
        var files = await _executor.ReadOnConnectionAsync(
            connection,
            "RESTORE FILELISTONLY FROM DISK = @baselinePath",
            new[] { new SqlParameter("@baselinePath", _options.BaselinePath) },
            BaselineProbeTimeoutSeconds,
            ct,
            BackupFileListReader.Read);

        if (files.Count == 0)
        {
            throw new InvalidOperationException(
                $"RESTORE FILELISTONLY returned no rows for '{_options.BaselinePath}'.");
        }
        return files;
    }

    private async Task ExecuteRestoreAsync(
        SqlConnection connection,
        string targetDbQuoted,
        string targetDbName,
        TargetFileSystem fs,
        IReadOnlyList<BackupFileMetadata> files,
        int restoreTimeout,
        CancellationToken ct)
    {
        var moves = MoveClauseBuilder.Build(files, targetDbName, fs, DateTime.UtcNow.Ticks);
        var restoreSql =
            $"RESTORE DATABASE {targetDbQuoted}\n"
            + "FROM DISK = @baselinePath\n"
            + "WITH REPLACE,\n"
            + moves.SqlFragment;

        var restoreParams = new List<SqlParameter>(moves.Parameters.Count + 1)
        {
            new("@baselinePath", _options.BaselinePath),
        };
        for (var i = 0; i < moves.Parameters.Count; i++)
        {
            var p = moves.Parameters[i];
            restoreParams.Add(new SqlParameter(p.Name, p.Value));
        }

        await _executor.ExecuteOnConnectionAsync(
            connection,
            restoreSql,
            restoreParams,
            restoreTimeout,
            ct);
    }

    private async Task RestoreUserAccessAsync(SqlConnection connection, string targetDbQuoted, CancellationToken ct)
    {
        await ExecAlterAsync(connection, $"ALTER DATABASE {targetDbQuoted} SET MULTI_USER", ct);
        await ExecAlterAsync(connection, $"ALTER DATABASE {targetDbQuoted} SET RECOVERY SIMPLE", ct);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Reads <c>state</c> and <c>user_access</c> from <c>sys.databases</c> in a single round-trip,
    /// then issues at most two ALTERs. The <see cref="RecoveryOutcome.Detail"/> string captures
    /// the observed values for diagnostic logging even when no action was taken.
    /// </remarks>
    public async Task<RecoveryOutcome> SelfHealStuckTargetAsync(string targetName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        SqlIdentifier.ValidateAgainstPattern(targetName, _targetNameRegex.Value);

        var (targetCs, targetDbName) = ResolveTargetConnection(targetName);

        var tgt = SqlIdentifier.Quote(targetDbName);
        var nonPooledMasterCs = BuildMasterCs(targetCs, pooling: false);

        await using var connection = await _executor.OpenAsync(nonPooledMasterCs, ct);

        var (state, userAccess) = await _executor.ReadOnConnectionAsync(
            connection,
            "SELECT state, user_access FROM sys.databases WHERE [name] = @targetName",
            new[] { new SqlParameter("@targetName", targetDbName) },
            DefaultCommandTimeoutSeconds,
            ct,
            ReadStateAndUserAccess);

        var recovered = false;
        var multiUser = false;

        if (state == DbStateRestoring)
        {
            await ExecAlterAsync(connection, $"RESTORE DATABASE {tgt} WITH RECOVERY", ct);
            recovered = true;
        }

        if (userAccess == DbUserAccessSingleUser)
        {
            await ExecAlterAsync(connection, $"ALTER DATABASE {tgt} SET MULTI_USER", ct);
            multiUser = true;
        }

        var action = (recovered, multiUser) switch
        {
            (true, true) => RecoveryAction.Both,
            (true, false) => RecoveryAction.RecoveredFromRestoring,
            (false, true) => RecoveryAction.RestoredMultiUser,
            _ => RecoveryAction.None,
        };

        var detail = string.Format(
            CultureInfo.InvariantCulture,
            "state={0}; user_access={1}; recovered={2}; multi_user_restored={3}",
            state,
            userAccess,
            recovered,
            multiUser);

        return new RecoveryOutcome(action, action != RecoveryAction.None, detail);
    }

    private string ResolveConnectionString(string name)
    {
        var cs = _configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException($"ConnectionStrings:{name} not configured.");
        }
        return cs;
    }

    /// <summary>
    /// Resolves a connection string by name and extracts the database name from its
    /// <c>InitialCatalog</c>. Throws if the entry is missing, blank, or has no <c>InitialCatalog</c>.
    /// </summary>
    private (string ConnectionString, string DatabaseName) ResolveTargetConnection(string connectionStringName)
    {
        var cs = ResolveConnectionString(connectionStringName);
        var builder = new SqlConnectionStringBuilder(cs);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{connectionStringName} must declare an InitialCatalog.");
        }
        return (cs, builder.InitialCatalog);
    }

    private Task ExecAlterAsync(SqlConnection connection, string sql, CancellationToken ct)
        => _executor.ExecuteOnConnectionAsync(
            connection,
            sql,
            Array.Empty<SqlParameter>(),
            DefaultCommandTimeoutSeconds,
            ct);

    private static string BuildMasterCs(string cs, bool pooling)
    {
        var b = new SqlConnectionStringBuilder(cs)
        {
            InitialCatalog = "master",
            Pooling = pooling,
        };
        return b.ConnectionString;
    }

    private static string BuildTargetDbCs(string cs, string targetDbName)
    {
        var b = new SqlConnectionStringBuilder(cs)
        {
            InitialCatalog = targetDbName,
            Pooling = true,
        };
        return b.ConnectionString;
    }

    private async Task<TargetFileSystem> ReadTargetFileSystemAsync(SqlConnection connection, CancellationToken ct)
    {
        var (dataPath, logPath) = await _executor.ReadOnConnectionAsync(
            connection,
            "SELECT "
            + "CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(260)) AS DataPath, "
            + "CAST(SERVERPROPERTY('InstanceDefaultLogPath')  AS NVARCHAR(260)) AS LogPath",
            Array.Empty<SqlParameter>(),
            DefaultCommandTimeoutSeconds,
            ct,
            reader =>
            {
                if (!reader.Read())
                {
                    return ((string?)null, (string?)null);
                }
                var data = reader["DataPath"];
                var log = reader["LogPath"];
                return (data is DBNull or null ? null : (string?)data,
                        log is DBNull or null ? null : (string?)log);
            });

        // Detect separator from whichever path is non-null. Both null → fallback applied later
        // from FILELISTONLY rows; for now default to '/' (overwritten in ApplyFilelistFallbackIfNeeded).
        var sample = dataPath ?? logPath ?? "/";
        var separator = TargetFileSystem.DetectSeparator(sample);
        return new TargetFileSystem(dataPath ?? string.Empty, logPath ?? string.Empty, separator);
    }

    private static TargetFileSystem ApplyFilelistFallbackIfNeeded(
        TargetFileSystem fs,
        IReadOnlyList<BackupFileMetadata> files)
    {
        if (!string.IsNullOrEmpty(fs.DataDir) && !string.IsNullOrEmpty(fs.LogDir))
        {
            return fs;
        }

        string? sampleData = null;
        string? sampleLog = null;
        for (var i = 0; i < files.Count; i++)
        {
            if (sampleData is null && files[i].Type == 'D')
            {
                sampleData = files[i].PhysicalName;
            }
            if (sampleLog is null && files[i].Type == 'L')
            {
                sampleLog = files[i].PhysicalName;
            }
            if (sampleData is not null && sampleLog is not null)
            {
                break;
            }
        }

        var separator = sampleData is not null
            ? TargetFileSystem.DetectSeparator(sampleData)
            : sampleLog is not null
                ? TargetFileSystem.DetectSeparator(sampleLog)
                : fs.Separator;

        var dataDir = string.IsNullOrEmpty(fs.DataDir)
            ? GetDirectoryFromPath(sampleData, separator) ?? string.Empty
            : fs.DataDir;
        var logDir = string.IsNullOrEmpty(fs.LogDir)
            ? GetDirectoryFromPath(sampleLog, separator) ?? string.Empty
            : fs.LogDir;

        return new TargetFileSystem(dataDir, logDir, separator);
    }

    private static string? GetDirectoryFromPath(string? path, char separator)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        var idx = path.LastIndexOf(separator);
        return idx < 0 ? null : path[..idx];
    }

    private async Task<int> ReadDatabaseStateAsync(SqlConnection connection, string targetDbName, CancellationToken ct)
    {
        var raw = await _executor.ScalarOnConnectionAsync(
            connection,
            "SELECT state FROM sys.databases WHERE [name] = @targetName",
            new[] { new SqlParameter("@targetName", targetDbName) },
            DefaultCommandTimeoutSeconds,
            ct);

        if (raw is null or DBNull)
        {
            return -1;
        }
        return Convert.ToInt32(raw, CultureInfo.InvariantCulture);
    }

    private static (int state, int userAccess) ReadStateAndUserAccess(IDataReader reader)
    {
        if (!reader.Read())
        {
            return (-1, -1);
        }
        var state = Convert.ToInt32(reader["state"], CultureInfo.InvariantCulture);
        var userAccess = Convert.ToInt32(reader["user_access"], CultureInfo.InvariantCulture);
        return (state, userAccess);
    }

    private async Task StampSourceMarkerAsync(string sourceDbCs, CancellationToken ct)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM sys.extended_properties
                           WHERE class = 0 AND major_id = 0 AND minor_id = 0
                             AND [name] = @prop AND CAST([value] AS NVARCHAR(MAX)) = @val)
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.extended_properties
                           WHERE class = 0 AND major_id = 0 AND minor_id = 0 AND [name] = @prop)
                    EXEC sp_updateextendedproperty @name = @prop, @value = @val;
                ELSE
                    EXEC sp_addextendedproperty    @name = @prop, @value = @val;
            END
            """;

        await _executor.ExecuteAsync(
            sourceDbCs,
            sql,
            new[]
            {
                new SqlParameter("@prop", _options.SourceMarker.Property),
                new SqlParameter("@val", _options.SourceMarker.Value),
            },
            DefaultCommandTimeoutSeconds,
            ct);
    }

    private async Task DropSourceMarkerAsync(string targetDbCs, CancellationToken ct)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM sys.extended_properties
                       WHERE class = 0 AND major_id = 0 AND minor_id = 0 AND [name] = @prop)
                EXEC sp_dropextendedproperty @name = @prop;
            """;

        await _executor.ExecuteAsync(
            targetDbCs,
            sql,
            new[] { new SqlParameter("@prop", _options.SourceMarker.Property) },
            DefaultCommandTimeoutSeconds,
            ct);
    }

    /// <summary>
    /// Best-effort <c>SET MULTI_USER</c> on a fresh non-pooled connection. Swallows any exception
    /// from the cleanup so the caller's <c>throw</c> rethrows the <i>original</i> failure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses <see cref="CancellationToken.None"/>: by the time we enter the catch block, the caller's
    /// token may already be cancelled — running cleanup under it would skip the recovery and leave
    /// the target stuck in <c>SINGLE_USER</c>. A faulted destructive verb must always attempt to
    /// release the single-user lock, regardless of cancellation state.
    /// </para>
    /// <para>
    /// Always opens a new connection: the primary connection from the <c>try</c> block may report
    /// <c>State == Open</c> but be broken (post-attention, severity ≥ 20). Reusing it would mask
    /// the original exception behind a misleading "Cleanup MULTI_USER failed".
    /// </para>
    /// </remarks>
    private async Task TryCleanupMultiUserAsync(
        string nonPooledMasterCs,
        string targetDbQuoted,
        string targetName)
    {
        // Always open a fresh non-pooled connection: the primary connection may report State==Open
        // but be broken (post-fault, post-attention, severity ≥20). Reusing it would mask the
        // original exception behind a misleading "Cleanup MULTI_USER failed".
        try
        {
            await using var conn = await _executor.OpenAsync(nonPooledMasterCs, CancellationToken.None);
            await ExecAlterAsync(conn, $"ALTER DATABASE {targetDbQuoted} SET MULTI_USER", CancellationToken.None);
        }
        catch (Exception cleanupEx)
        {
            _logger.LogError(
                cleanupEx,
                "Cleanup MULTI_USER failed during {TargetName} restore; original exception will be rethrown",
                targetName);
        }
    }
}
