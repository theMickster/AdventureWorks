namespace AdventureWorks.UnitTests.Setup;

/// <summary>
/// Base class for all unit tests. 
/// All unit tests should inherit from this class or another abstract class in this class's hierarchy.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Simplification", "CLASS0001:Seal class", Justification = "This analyzer is wrong in this use-case.")]
public abstract class UnitTestBase : TestBase
{
    public DateTime DefaultAuditDate => new DateTime(2011, 11, 11, 11, 11, 11);

    protected UnitTestBase()
    {
        Setup();
    }
}