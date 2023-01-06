using Newtonsoft.Json;
using System.Runtime.Serialization;

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

    protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        _errorMessages = info.GetValue(nameof(_errorMessages), typeof(IEnumerable<string>)) as IEnumerable<string> ?? Array.Empty<string>();
    }

    #endregion Constructors


    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue(nameof(_errorMessages), _errorMessages);
    }

    public override string ToString()
    {
        var text = _errorMessages.Any() ? string.Join(";", _errorMessages) : "N/A";

        return $"ConfigurationException({Message}: {text})";
    }
}