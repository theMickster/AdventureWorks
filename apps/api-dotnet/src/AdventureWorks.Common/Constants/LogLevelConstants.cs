using Microsoft.Extensions.Logging;

namespace AdventureWorks.Common.Constants;

public static class LogLevelConstants
{
    /// <summary>
    /// Default LogLevel for Logger:LogLevel:Default.
    /// </summary>
    /// <remarks>
    /// This is intended to be the default if a value is not provided
    /// </remarks>
    public const LogLevel BaseLogLevelDefault = LogLevel.Information;

    /// <summary>
    /// Default LogLevel for Logger:LogLevel:Microsoft.
    /// </summary>
    /// <remarks>
    /// This is intended to be the default if a value is not provided
    /// </remarks>
    public const LogLevel BaseLogLevelMicrosoft = LogLevel.Warning;

    /// <summary>
    /// Default LogLevel for Logger:LogLevel:Microsoft.Hosting.Lifetime.
    /// </summary>
    /// <remarks>
    /// This is intended to be the default if a value is not provided
    /// </remarks>
    public const LogLevel BaseLogLevelMicrosoftHostingLifetime = LogLevel.Information;

    /// <summary>
    /// Default LogLevel for Logger:LogLevel:Microsoft.Hosting.Lifetime.
    /// </summary>
    /// <remarks>
    /// This is intended to be the default if a value is not provided
    /// </remarks>
    public const LogLevel EventSourceLogLevelDefault = LogLevel.Warning;

}
