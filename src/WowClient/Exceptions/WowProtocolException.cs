namespace WowClient.Exceptions;

/// <summary>
/// Raised when Blizzard returns a successful HTTP response that cannot be interpreted as a valid
/// API document, for example an empty body where a document was expected.
/// </summary>
public sealed class WowProtocolException : Exception
{
    public WowProtocolException(string message, Uri? requestUri = null, Exception? innerException = null)
        : base(message, innerException)
    {
        RequestUri = requestUri;
    }

    /// <summary>Request URI that produced the invalid response.</summary>
    public Uri? RequestUri { get; }
}
