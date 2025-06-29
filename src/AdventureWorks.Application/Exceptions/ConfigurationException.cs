using Newtonsoft.Json;

namespace AdventureWorks.Application.Exceptions;

public class ConfigurationException : Exception
{
    private readonly IEnumerable<string> _errorMessages = new List<string>();

    [JsonIgnore]
    public IEnumerable<string> ErrorMessages => _errorMessages;

    #region Constructors

    public ConfigurationException()
    {

    }

    public ConfigurationException(string message) : base(message)
    {

    }

    public ConfigurationException(
        IEnumerable<string> errorMessages,
        string message) : base(message) => _errorMessages = errorMessages;

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ConfigurationException(
        IEnumerable<string> errorMessages,
        string message,
        Exception innerException) : base(message, innerException)
        => _errorMessages = errorMessages;
    
    #endregion Constructors

    public override string ToString()
    {
        var text = _errorMessages.Any() ? string.Join(";", _errorMessages) : "N/A";

        return $"ConfigurationException({Message}: {text})";
    }
}