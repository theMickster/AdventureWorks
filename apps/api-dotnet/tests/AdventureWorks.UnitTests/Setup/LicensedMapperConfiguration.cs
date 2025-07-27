using AdventureWorks.Common.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdventureWorks.UnitTests.Setup;

[ExcludeFromCodeCoverage]
public sealed class LicensedMapperConfiguration
{
    private static readonly Lazy<string> CachedLicenseKey = new(() =>
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var key = configuration[ConfigurationConstants.AutoMapperLicenseKey];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                $"The '{ConfigurationConstants.AutoMapperLicenseKey}' configuration value is required for AutoMapper unit tests.");
        }

        return key;
    });

    private readonly AutoMapper.MapperConfiguration _inner;

    public LicensedMapperConfiguration(Action<IMapperConfigurationExpression> configure)
        : this(configure, NullLoggerFactory.Instance)
    {
    }

    public LicensedMapperConfiguration(Action<IMapperConfigurationExpression> configure, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var autoMapperLicenseKey = CachedLicenseKey.Value;

        _inner = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.LicenseKey = autoMapperLicenseKey;
            configure(cfg);
        }, loggerFactory);
    }

    public IMapper CreateMapper() => _inner.CreateMapper();

    public void AssertConfigurationIsValid() => _inner.AssertConfigurationIsValid();
}
