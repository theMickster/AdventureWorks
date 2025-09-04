namespace AdventureWorks.Application.Exceptions;

/// <summary>Thrown when a request conflicts with the current state of a resource.</summary>
public sealed class ConflictException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public ConflictException()
    {
    }

    /// <summary>Initializes a new instance with the specified message.</summary>
    /// <param name="message">The message describing the conflict.</param>
    public ConflictException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance with the specified message and inner exception.</summary>
    /// <param name="message">The message describing the conflict.</param>
    /// <param name="innerException">The exception that caused this conflict.</param>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
