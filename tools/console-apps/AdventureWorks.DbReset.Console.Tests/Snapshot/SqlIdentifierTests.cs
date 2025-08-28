using System.Text.RegularExpressions;
using AdventureWorks.DbReset.Console.Snapshot.Internal;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Snapshot;

public sealed class SqlIdentifierTests
{
    [Fact]
    public void Quote_WrapsIdentifierInBrackets()
    {
        SqlIdentifier.Quote("AdventureWorks_E2E").Should().Be("[AdventureWorks_E2E]");
    }

    [Fact]
    public void Quote_DoublesClosingBracket()
    {
        SqlIdentifier.Quote("a]b").Should().Be("[a]]b]");
    }

    [Fact]
    public void Quote_DoublesAdjacentClosingBrackets()
    {
        SqlIdentifier.Quote("a]]b").Should().Be("[a]]]]b]");
    }

    [Fact]
    public void Quote_NullThrowsArgumentException()
    {
        Action act = () => SqlIdentifier.Quote(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quote_EmptyThrowsArgumentException()
    {
        Action act = () => SqlIdentifier.Quote(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quote_WhitespaceOnlyThrowsArgumentException()
    {
        Action act = () => SqlIdentifier.Quote("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quote_OverSysnameLimitThrowsArgumentException()
    {
        var tooLong = new string('a', 129);

        Action act = () => SqlIdentifier.Quote(tooLong);

        act.Should().Throw<ArgumentException>().WithMessage("*sysname*");
    }

    [Fact]
    public void Quote_AtSysnameLimitSucceeds()
    {
        var atLimit = new string('a', 128);

        var result = SqlIdentifier.Quote(atLimit);

        result.Should().Be($"[{atLimit}]");
    }

    [Theory]
    [InlineData("a\0b")]
    [InlineData("a\rb")]
    [InlineData("a\nb")]
    public void Quote_RejectsControlCharacters(string input)
    {
        Action act = () => SqlIdentifier.Quote(input);
        act.Should().Throw<ArgumentException>().WithMessage("*forbidden*");
    }

    [Fact]
    public void ValidateAgainstPattern_WhenMatch_DoesNotThrow()
    {
        var pattern = new Regex("^AdventureWorks_(E2E|Test)$");

        Action act = () => SqlIdentifier.ValidateAgainstPattern("AdventureWorks_E2E", pattern);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAgainstPattern_WhenMismatch_ThrowsWithIdentifierAndPattern()
    {
        var pattern = new Regex("^AdventureWorks_(E2E|Test)$");

        Action act = () => SqlIdentifier.ValidateAgainstPattern("master", pattern);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*master*");
    }

    [Fact]
    public void ValidateAgainstPattern_NullName_ThrowsArgumentNull()
    {
        var pattern = new Regex(".*");

        Action act = () => SqlIdentifier.ValidateAgainstPattern(null!, pattern);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAgainstPattern_NullPattern_ThrowsArgumentNull()
    {
        Action act = () => SqlIdentifier.ValidateAgainstPattern("x", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------- Additional QA coverage --------------------

    [Fact]
    public void Quote_CanonicalEscape_DoublesClosingBracket()
    {
        // T-SQL canonical escape: ] inside an identifier is doubled to ]].
        SqlIdentifier.Quote("A]B").Should().Be("[A]]B]");
    }

    [Fact]
    public void ValidateAgainstPattern_ProdPattern_RejectsAdventureWorksDev()
    {
        // The default TargetNamePattern is ^AdventureWorks_E2E$ (or similar opt-in form). The prod
        // database name 'AdventureWorksDev' must NOT match — the validator's job is to refuse to
        // operate on names that don't carry the test-environment prefix/underscore convention.
        var pattern = new Regex("^AdventureWorks_(E2E|Test)$");

        Action act = () => SqlIdentifier.ValidateAgainstPattern("AdventureWorksDev", pattern);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*AdventureWorksDev*");
    }

    [Fact]
    public void Quote_AcceptsUnicodeIdentifier()
    {
        // SQL Server accepts Unicode identifiers; Quote only rejects \0\r\n control chars.
        // Pattern-level allowlist is the right place to enforce ASCII/word characters; Quote does not.
        SqlIdentifier.Quote("ÄdventureWörks").Should().Be("[ÄdventureWörks]");
    }
}
