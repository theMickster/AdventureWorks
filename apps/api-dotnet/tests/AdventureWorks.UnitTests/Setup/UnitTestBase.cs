using AdventureWorks.Application.Helpers;

namespace AdventureWorks.UnitTests.Setup;

/// <summary>
/// Base class for all unit tests.
/// All unit tests should inherit from this class or another abstract class in this class's hierarchy.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Simplification", "CLASS0001:Seal class", Justification = "This analyzer is wrong in this use-case.")]
public abstract class UnitTestBase : TestBase
{
    public static DateTime DefaultAuditDate => new(2011, 11, 11, 11, 11, 11, DateTimeKind.Utc);

    /// <summary>
    /// Every test class that scans the whole Application assembly for AutoMapper profiles
    /// (config.AddMaps(typeof(AnyProfile).Assembly)) builds an identical configuration, since the
    /// anchor type only determines which assembly is scanned, not which profiles are included.
    /// Building that scan once here instead of once per test class (or, previously, once per test
    /// method) avoids thousands of redundant AutoMapper configuration builds per run.
    /// </summary>
    internal static readonly MapperConfiguration SharedMapperConfiguration =
        CreateMapperConfiguration(c => c.AddMaps(typeof(ApplicationServiceRegistration).Assembly));

    internal static readonly IMapper SharedMapper = SharedMapperConfiguration.CreateMapper();

    protected static MapperConfiguration CreateMapperConfiguration(Action<IMapperConfigurationExpression> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        return new MapperConfiguration(configure);
    }

    protected UnitTestBase()
    {
        Setup();
    }
}