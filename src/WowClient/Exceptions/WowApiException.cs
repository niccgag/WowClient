using System.Net;

namespace WowClient.Exceptions;

/// <summary>
/// Raised when the Blizzard API returns a non-success status code.
/// </summary>
public sealed class WowApiException : Exception
{
    public WowApiException(
        HttpStatusCode statusCode,
        int? errorCode,
        string? errorType,
        string? errorDetail,
        Uri? requestUri,
        string? responseBody)
        : base(BuildMessage(statusCode, errorCode, errorType, errorDetail, requestUri))
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorType = errorType;
        ErrorDetail = errorDetail;
        RequestUri = requestUri;
        ResponseBody = responseBody;
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

    private static string BuildMessage(
        HttpStatusCode statusCode,
        int? errorCode,
        string? errorType,
        string? errorDetail,
        Uri? requestUri)
    {
        var message = $"Blizzard API request failed with status {(int)statusCode} ({statusCode})";

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
