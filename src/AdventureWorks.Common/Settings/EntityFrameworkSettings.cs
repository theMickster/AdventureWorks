namespace AdventureWorks.Common.Settings;
public class EntityFrameworkCoreSettings
{
    public const string SettingsRootName = "EntityFrameworkCoreSettings";

    public string CommandLogLevel { get; set; }

    public int CommandTimeout { get; set; }

    public string CurrentConnectionStringName { get; set; }

    public List<DatabaseConnectionString> DatabaseConnectionStrings { get; set; }
}
