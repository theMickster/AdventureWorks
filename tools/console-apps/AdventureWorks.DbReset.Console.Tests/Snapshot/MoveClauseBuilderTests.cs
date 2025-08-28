using AdventureWorks.DbReset.Console.Snapshot;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Snapshot;

public sealed class MoveClauseBuilderTests
{
    private const long DeterministicTicks = 42L;

    private static TargetFileSystem ForwardSlashFs(string data = "/var/opt/mssql/data", string log = "/var/opt/mssql/data")
        => new(data, log, '/');

    private static TargetFileSystem BackslashFs(string data = @"C:\SQL\Data", string log = @"C:\SQL\Log")
        => new(data, log, '\\');

    [Fact]
    public void Build_OneDataOneLog_EmitsMdfAndLdf()
    {
        var files = new List<BackupFileMetadata>
        {
            new("AdventureWorks", "/old/aw.mdf", 'D', 100L),
            new("AdventureWorks_log", "/old/aw.ldf", 'L', 50L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), DeterministicTicks);

        result.Parameters.Should().HaveCount(4);
        result.Parameters[0].Should().Be(new MoveClauseParameter("@logical0", "AdventureWorks"));
        result.Parameters[1].Should().Be(new MoveClauseParameter("@physical0", "/var/opt/mssql/data/MyDb.mdf"));
        result.Parameters[2].Should().Be(new MoveClauseParameter("@logical1", "AdventureWorks_log"));
        result.Parameters[3].Should().Be(new MoveClauseParameter("@physical1", "/var/opt/mssql/data/MyDb_log.ldf"));
        result.SqlFragment.Should().Contain("MOVE @logical0 TO @physical0");
        result.SqlFragment.Should().Contain("MOVE @logical1 TO @physical1");
    }

    [Fact]
    public void Build_TwoDataOneLog_NamesSecondaryDataAsNdf()
    {
        var files = new List<BackupFileMetadata>
        {
            new("D1", "/old/d1.mdf", 'D', 10L),
            new("D2", "/old/d2.ndf", 'D', 20L),
            new("L1", "/old/l1.ldf", 'L', 5L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), DeterministicTicks);

        var paths = result.Parameters
            .Where(p => p.Name.StartsWith("@physical", StringComparison.Ordinal))
            .Select(p => p.Value)
            .ToList();
        paths.Should().Equal(
            "/var/opt/mssql/data/MyDb.mdf",
            "/var/opt/mssql/data/MyDb_1.ndf",
            "/var/opt/mssql/data/MyDb_log.ldf");
    }

    [Fact]
    public void Build_DataLogFilestream_AppendsTicksToFilestreamSuffix()
    {
        var files = new List<BackupFileMetadata>
        {
            new("D1", "/old/d1.mdf", 'D', 10L),
            new("L1", "/old/l1.ldf", 'L', 5L),
            new("FS1", "/old/fs1", 'S', 1L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), 42L);

        var fsPhysical = result.Parameters
            .First(p => p.Name == "@physical2")
            .Value;
        fsPhysical.Should().Be("/var/opt/mssql/data/MyDb_FS0_42");
    }

    [Fact]
    public void Build_FilestreamSuffixDeterministicForInjectedTicks()
    {
        var files = new List<BackupFileMetadata>
        {
            new("FS1", "/old/fs1", 'S', 1L),
        };

        var resultA = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), 100L);
        var resultB = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), 200L);

        resultA.Parameters[1].Value.Should().Be("/var/opt/mssql/data/MyDb_FS0_100");
        resultB.Parameters[1].Value.Should().Be("/var/opt/mssql/data/MyDb_FS0_200");
    }

    [Fact]
    public void Build_BackslashSeparator_EmitsWindowsPaths()
    {
        var files = new List<BackupFileMetadata>
        {
            new("D1", @"C:\old\d1.mdf", 'D', 10L),
            new("L1", @"C:\old\l1.ldf", 'L', 5L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", BackslashFs(), DeterministicTicks);

        result.Parameters[1].Value.Should().Be(@"C:\SQL\Data\MyDb.mdf");
        result.Parameters[3].Value.Should().Be(@"C:\SQL\Log\MyDb_log.ldf");
    }

    [Fact]
    public void Build_ForwardSlashSeparator_EmitsLinuxPaths()
    {
        var files = new List<BackupFileMetadata>
        {
            new("D1", "/old/d1.mdf", 'D', 10L),
            new("L1", "/old/l1.ldf", 'L', 5L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), DeterministicTicks);

        result.Parameters[1].Value.Should().Be("/var/opt/mssql/data/MyDb.mdf");
        result.Parameters[3].Value.Should().Be("/var/opt/mssql/data/MyDb_log.ldf");
    }

    [Fact]
    public void Build_DataDirEndsWithSeparator_DoesNotDoubleSeparator()
    {
        var fs = new TargetFileSystem("/var/opt/mssql/data/", "/var/opt/mssql/data/", '/');
        var files = new List<BackupFileMetadata>
        {
            new("D1", "/old/d1.mdf", 'D', 10L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", fs, DeterministicTicks);

        result.Parameters[1].Value.Should().Be("/var/opt/mssql/data/MyDb.mdf");
    }

    [Fact]
    public void Build_UnsupportedType_ThrowsNotSupported()
    {
        var files = new List<BackupFileMetadata>
        {
            new("X", "/old/x", 'X', 1L),
        };

        Action act = () => MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), DeterministicTicks);

        act.Should().Throw<NotSupportedException>()
            .WithMessage("*FILELISTONLY Type 'X'*");
    }

    [Fact]
    public void Build_EmptyFiles_ReturnsEmptyFragmentAndEmptyParameters()
    {
        var result = MoveClauseBuilder.Build(
            Array.Empty<BackupFileMetadata>(),
            "MyDb",
            ForwardSlashFs(),
            DeterministicTicks);

        result.SqlFragment.Should().BeEmpty();
        result.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void Build_NullFiles_ThrowsArgumentNull()
    {
        Action act = () => MoveClauseBuilder.Build(null!, "MyDb", ForwardSlashFs(), DeterministicTicks);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Build_EmptyTargetDb_ThrowsArgument()
    {
        Action act = () => MoveClauseBuilder.Build(
            new List<BackupFileMetadata>(),
            string.Empty,
            ForwardSlashFs(),
            DeterministicTicks);
        act.Should().Throw<ArgumentException>();
    }

    // -------------------- Additional QA coverage --------------------

    [Fact]
    public void Build_FilestreamPhysical_DoesNotEndInPathSeparator()
    {
        // SQL Server creates the FILESTREAM directory; the MOVE target must be a directory NAME,
        // not a directory PATH ending in '/'. (The directory itself is created from this name.)
        var files = new List<BackupFileMetadata>
        {
            new("FS1", "/old/fs1", 'S', 1L),
        };

        var resultFwd = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), 999L);
        var resultBack = MoveClauseBuilder.Build(files, "MyDb", BackslashFs(), 999L);

        var fwdPath = resultFwd.Parameters[1].Value;
        var backPath = resultBack.Parameters[1].Value;

        fwdPath.Should().NotEndWith("/");
        backPath.Should().NotEndWith("\\");
        // Directory name itself ends with the deterministic ticks suffix.
        fwdPath.Should().EndWith("_FS0_999");
        backPath.Should().EndWith("_FS0_999");
    }

    [Fact]
    public void Build_BackslashSeparator_FilestreamUsesBackslash()
    {
        // Pure assertion that FILESTREAM physical paths respect the detected separator.
        var files = new List<BackupFileMetadata>
        {
            new("FS1", @"C:\old\fs1", 'S', 1L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", BackslashFs(), 7L);

        result.Parameters[1].Value.Should().Be(@"C:\SQL\Data\MyDb_FS0_7");
        result.Parameters[1].Value.Should().NotContain("/");
    }

    [Fact]
    public void Build_TwoLogs_SecondaryLogUsesIndexedSuffix()
    {
        // The naming convention from the principal direction:
        //   first log → <db>_log.ldf
        //   subsequent → <db>_log_<n>.ldf
        var files = new List<BackupFileMetadata>
        {
            new("L1", "/old/l1.ldf", 'L', 5L),
            new("L2", "/old/l2.ldf", 'L', 5L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), DeterministicTicks);

        result.Parameters[1].Value.Should().Be("/var/opt/mssql/data/MyDb_log.ldf");
        result.Parameters[3].Value.Should().Be("/var/opt/mssql/data/MyDb_log_1.ldf");
    }

    [Fact]
    public void Build_FirstDataIsMdf_AdditionalDataAreNdf()
    {
        // Explicit assertion of the .mdf vs .ndf naming convention.
        var files = new List<BackupFileMetadata>
        {
            new("D0", "/old/d0", 'D', 1L),
            new("D1", "/old/d1", 'D', 1L),
            new("D2", "/old/d2", 'D', 1L),
        };

        var result = MoveClauseBuilder.Build(files, "MyDb", ForwardSlashFs(), DeterministicTicks);

        var paths = result.Parameters
            .Where(p => p.Name.StartsWith("@physical", StringComparison.Ordinal))
            .Select(p => p.Value)
            .ToList();

        paths[0].Should().EndWith("MyDb.mdf");
        paths[1].Should().EndWith("MyDb_1.ndf");
        paths[2].Should().EndWith("MyDb_2.ndf");
    }
}
