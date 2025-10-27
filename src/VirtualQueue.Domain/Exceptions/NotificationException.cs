namespace VirtualQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when notification operations fail.
/// </summary>
public class NotificationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotificationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NotificationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
