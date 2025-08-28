using System.Data;
using AdventureWorks.DbReset.Console.Snapshot;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Snapshot;

public sealed class BackupFileListReaderTests
{
    private static IDataReader BuildReader(params (string Logical, string Physical, string Type, long Size)[] rows)
    {
        var dt = new DataTable();
        dt.Columns.Add("LogicalName", typeof(string));
        dt.Columns.Add("PhysicalName", typeof(string));
        dt.Columns.Add("Type", typeof(string));
        dt.Columns.Add("Size", typeof(long));
        foreach (var r in rows)
        {
            dt.Rows.Add(r.Logical, r.Physical, r.Type, r.Size);
        }
        return dt.CreateDataReader();
    }

    [Fact]
    public void Read_SingleDataAndLog_ReturnsTwoRows()
    {
        using var reader = BuildReader(
            ("AdventureWorks", "/var/opt/mssql/data/AdventureWorks.mdf", "D", 100L),
            ("AdventureWorks_log", "/var/opt/mssql/data/AdventureWorks_log.ldf", "L", 50L));

        var result = BackupFileListReader.Read(reader);

        result.Should().HaveCount(2);
        result[0].LogicalName.Should().Be("AdventureWorks");
        result[0].Type.Should().Be('D');
        result[0].Size.Should().Be(100L);
        result[1].Type.Should().Be('L');
    }

    [Fact]
    public void Read_MultipleDataPlusLog_ReturnsAllRowsInOrder()
    {
        using var reader = BuildReader(
            ("D1", "/data/d1.mdf", "D", 10L),
            ("D2", "/data/d2.ndf", "D", 20L),
            ("L1", "/data/l1.ldf", "L", 5L));

        var result = BackupFileListReader.Read(reader);

        result.Should().HaveCount(3);
        result.Select(r => r.LogicalName).Should().Equal("D1", "D2", "L1");
        result.Select(r => r.Type).Should().Equal('D', 'D', 'L');
    }

    [Fact]
    public void Read_AllFourTypes_ParsesEachCorrectly()
    {
        using var reader = BuildReader(
            ("D1", "/data/d1.mdf", "D", 10L),
            ("L1", "/data/l1.ldf", "L", 5L),
            ("FS1", "/data/fs1", "S", 1L),
            ("D2", "/data/d2.ndf", "D", 20L));

        var result = BackupFileListReader.Read(reader);

        result.Should().HaveCount(4);
        result.Select(r => r.Type).Should().Equal('D', 'L', 'S', 'D');
    }

    [Fact]
    public void Read_FilestreamRowOnly_ConvertsTypeToCharS()
    {
        using var reader = BuildReader(("FS", "/data/fs", "S", 1L));

        var result = BackupFileListReader.Read(reader);

        result.Should().ContainSingle();
        result[0].Type.Should().Be('S');
    }

    [Fact]
    public void Read_EmptyReader_ReturnsEmptyList()
    {
        using var reader = BuildReader();

        var result = BackupFileListReader.Read(reader);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Read_MissingLogicalNameColumn_ThrowsInvalidOperation()
    {
        var dt = new DataTable();
        dt.Columns.Add("PhysicalName", typeof(string));
        dt.Columns.Add("Type", typeof(string));
        dt.Columns.Add("Size", typeof(long));
        dt.Rows.Add("/p", "D", 1L);
        using var reader = dt.CreateDataReader();

        Action act = () => BackupFileListReader.Read(reader);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RESTORE FILELISTONLY*required column*");
    }

    [Fact]
    public void Read_NullReader_ThrowsArgumentNull()
    {
        Action act = () => BackupFileListReader.Read(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------- Additional QA coverage --------------------

    [Fact]
    public void Read_SizeColumnAsDecimal_ConvertsToLong()
    {
        // RESTORE FILELISTONLY's Size column is numeric(20, 0) — the SQL Server provider returns it
        // as decimal. Convert.ToInt64 handles decimal correctly; this test guards against a future
        // refactor that changes to a direct cast (which would throw InvalidCastException).
        var dt = new DataTable();
        dt.Columns.Add("LogicalName", typeof(string));
        dt.Columns.Add("PhysicalName", typeof(string));
        dt.Columns.Add("Type", typeof(string));
        dt.Columns.Add("Size", typeof(decimal));
        dt.Rows.Add("AdventureWorks", "/data/aw.mdf", "D", 12345678901234m);
        using var reader = dt.CreateDataReader();

        var result = BackupFileListReader.Read(reader);

        result.Should().ContainSingle();
        result[0].Size.Should().Be(12345678901234L);
    }

    [Fact]
    public void Read_LargeInt64Size_PreservedExactly()
    {
        // Defensive: int64 values near the upper end must not lose precision through Convert.ToInt64.
        var dt = new DataTable();
        dt.Columns.Add("LogicalName", typeof(string));
        dt.Columns.Add("PhysicalName", typeof(string));
        dt.Columns.Add("Type", typeof(string));
        dt.Columns.Add("Size", typeof(long));
        dt.Rows.Add("Big", "/data/big.mdf", "D", long.MaxValue - 1);
        using var reader = dt.CreateDataReader();

        var result = BackupFileListReader.Read(reader);

        result[0].Size.Should().Be(long.MaxValue - 1);
    }
}
