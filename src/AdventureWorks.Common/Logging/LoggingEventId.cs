using System.ComponentModel.DataAnnotations;

namespace AdventureWorks.Common.Logging;

/// <summary>
/// This enumeration lists all the possible event identifier codes for event logging along with a 
/// friendly display name (uses the DisplayName attribute). 
/// Do not modify the numeric enum values because they are stored in event logs. 
/// New values may be appended to the list as required.
/// </summary>
public enum LoggingEventId
{
    // System (1000 - 1999)
    [Display(Name = "Application Startup")]
    ApplicationStartup = 1000,
    [Display(Name = "Application Shutdown")]
    ApplicationShutdown
}
