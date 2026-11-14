using System.Net;

namespace WowClient.Exceptions;

/// <summary>
/// Raised when the Blizzard API returns a non-success status code, or a success status whose body
/// cannot be interpreted as a valid API document (for example malformed JSON).
/// </summary>
public sealed class WowApiException : Exception
{
    public WowApiException(
        HttpStatusCode statusCode,
        int? errorCode,
        string? errorType,
        string? errorDetail,
        Uri? requestUri,
        string? responseBody,
        TimeSpan? retryAfter = null,
        Exception? innerException = null)
        : base(BuildMessage(statusCode, errorCode, errorType, errorDetail, requestUri), innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorType = errorType;
        ErrorDetail = errorDetail;
        RequestUri = requestUri;
        ResponseBody = responseBody;
        RetryAfter = retryAfter;
    }

    /// <summary>HTTP status returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Blizzard error code, when the response included one.</summary>
    public int? ErrorCode { get; }

    /// <summary>Blizzard error type, when the response included one.</summary>
    public string? ErrorType { get; }

    /// <summary>Human-readable error detail returned by Blizzard.</summary>
    public string? ErrorDetail { get; }

    /// <summary>Request URI that produced the error.</summary>
    public Uri? RequestUri { get; }

    /// <summary>Raw response body, useful for diagnostics.</summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Delay requested by the API through the <c>Retry-After</c> header, when one was present.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    private static string BuildMessage(
        HttpStatusCode statusCode,
        int? errorCode,
        string? errorType,
        string? errorDetail,
        Uri? requestUri)
    {
        var message = (int)statusCode is >= 200 and < 300
            ? $"Blizzard API returned an invalid response with status {(int)statusCode} ({statusCode})"
            : $"Blizzard API request failed with status {(int)statusCode} ({statusCode})";

        if (!string.IsNullOrWhiteSpace(errorDetail))
        {
            message += $": {errorDetail}";
        }

        if (errorCode is not null)
        {
            message += $" [code: {errorCode}]";
        }

        if (!string.IsNullOrWhiteSpace(errorType))
        {
            message += $" [type: {errorType}]";
        }

        if (requestUri is not null)
        {
            message += $" ({requestUri})";
        }

        return message;
    }
}
