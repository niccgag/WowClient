using System.Net;

namespace WowClient.Exceptions;

/// <summary>
/// Raised when a Battle.net OAuth token cannot be obtained.
/// </summary>
public sealed class WowAuthenticationException : Exception
{
    public WowAuthenticationException(
        string message,
        HttpStatusCode? statusCode = null,
        string? error = null,
        string? errorDescription = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Error = error;
        ErrorDescription = errorDescription;
    }

    /// <summary>HTTP status returned by the OAuth endpoint, when a response was received.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>OAuth error code, such as <c>invalid_client</c>.</summary>
    public string? Error { get; }

    /// <summary>OAuth error description.</summary>
    public string? ErrorDescription { get; }
}
