namespace AdventureWorks.UnitTests.Setup.Fixtures;

[SuppressMessage("Simplification", "CLASS0001:Seal class", Justification = "Because I said so...")]
internal abstract class UnitTestFixtureBase
{
    internal static DateTime StandardModifiedDate = new(2021, 11, 11, 11, 15, 07);
}
