namespace AdventureWorks.Common.Constants;

public static class ConfigurationConstants
{
    /// <summary>
    /// AKV URL from injected Environment Variables -OR- User Secrets
    /// </summary>
    public const string KeyVaultUrl = "KeyVault:VaultUri";

    /// <summary>
    /// The AKV initial retry delay, defaults to 100 ms
    /// </summary>
    public const string KeyVaultRetryDelay = "KeyVault:RetryDelayMilliseconds";

    /// <summary>
    /// The maximum amount of time that will be spent retrying connecting to AKV, default to 3000ms
    /// </summary>
    public const string KeyVaultMaxRetryDelay = "KeyVault:MaxRetryDelayMilliseconds";

    /// <summary>
    /// The maximum number of retry attempts to connect to AKV, default to 5
    /// </summary>
    public const string KeyVaultMaxRetryAttempts = "KeyVault:MaxRetryAttempts";

    /// <summary>
    /// AppInsightsInstrumentationKey3 value name
    /// </summary>
    public const string AppInsightsConnectionString = "ApplicationInsights:ConnectionString";

    /// <summary>
    /// ASPNetCore Environment1 value name
    /// </summary>
    public const string ApplicationEnvironment01 = "ASPNETCORE_ENVIRONMENT";

    /// <summary>
    /// ASPNetCore Environment2 value name
    /// </summary>
    public const string ApplicationEnvironment02 = "APPSETTING_ASPNETCORE_ENVIRONMENT";

    /// <summary>
    /// Default SQL Server Connection string name
    /// </summary>
    public const string SqlConnectionDefaultConnectionName = "DefaultConnection";

    /// <summary>
    /// Azure SQL Server Connection string name
    /// </summary>
    public const string SqlConnectionSqlAzureConnectionName = "SqlAzureConnection";

    /// <summary>
    /// The configuration key used for setting the default Sql connection string
    /// </summary>
    public const string CurrentConnectionStringNameKey = "CurrentConnectionStringName";

    /// <summary>
    /// The HTTP header name for correlation ID tracking across distributed systems
    /// </summary>
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
}