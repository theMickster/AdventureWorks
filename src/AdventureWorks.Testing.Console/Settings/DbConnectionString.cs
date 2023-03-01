namespace AdventureWorks.Testing.Console.Settings;

internal static class DbConnectionString
{
    private static string _connectionString = "Server=.; Database=HelloWorld; Application Name=TestTestTest;";

    public static string? ConnectionStringName { get; set; }

    public static string ConnectionString
    {
        get => _connectionString;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value), "The connection string cannot be null or empty");
            }

            if (!value.ToLower().Contains("server"))
            {
                throw new InvalidOperationException("The connection string must contain a 'Server' attribute.");
            }

            if (!value.ToLower().Contains("database") && !value.ToLower().Contains("initial catalog"))
            {
                throw new InvalidOperationException("The connection string must contain a 'database' or 'initial catalog' attribute.");
            }

            if (!value.ToLower().Contains("application name"))
            {
                throw new InvalidOperationException("The connection string must contain a 'application name' attribute to distinguish it from other database connections.");
            }

            _connectionString = value;

        }
    }
}
