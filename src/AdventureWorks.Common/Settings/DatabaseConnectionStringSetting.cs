namespace AdventureWorks.Common.Settings;

public class DatabaseConnectionString
{
    private string _connectionString = "Server=.; Database=HelloWorld; Application Name=TestTestTest;";

    public string? ConnectionStringName { get; set; }

    public string ConnectionString
    {
        get => _connectionString;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value), "The connection string cannot be null or empty");
            }

            if (!value.Contains("server", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("The connection string must contain a 'Server' attribute.");
            }

            if (!value.Contains("database", StringComparison.CurrentCultureIgnoreCase) && !value.Contains("initial catalog", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("The connection string must contain a 'database' or 'initial catalog' attribute.");
            }

            if (!value.Contains("application name", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("The connection string must contain a 'application name' attribute to distinguish it from other database connections.");
            }

            _connectionString = value;

        }
    }
}