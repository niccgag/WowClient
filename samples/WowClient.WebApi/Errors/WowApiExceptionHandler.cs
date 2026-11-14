using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using WowClient.Exceptions;

namespace WowClient.WebApi.Errors;

/// <summary>
/// Converts Blizzard client failures into RFC 7807 problem responses without leaking credentials.
/// </summary>
public sealed class WowApiExceptionHandler(ILogger<WowApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case WowApiException apiException:
                await WriteProblemAsync(
                    httpContext,
                    MapStatusCode(apiException.StatusCode),
                    "Blizzard API request failed",
                    apiException.ErrorDetail ?? apiException.Message,
                    new Dictionary<string, object?>
                    {
                        ["blizzardCode"] = apiException.ErrorCode,
                        ["blizzardType"] = apiException.ErrorType,
                        ["requestUri"] = apiException.RequestUri?.ToString(),
                    },
                    apiException.RetryAfter);
                return true;

            case WowAuthenticationException authenticationException:
                await WriteProblemAsync(
                    httpContext,
                    StatusCodes.Status502BadGateway,
                    "Blizzard authentication failed",
                    authenticationException.Message,
                    null);
                return true;

            case WowProtocolException protocolException:
                await WriteProblemAsync(
                    httpContext,
                    StatusCodes.Status502BadGateway,
                    "Blizzard API returned an invalid response",
                    protocolException.Message,
                    new Dictionary<string, object?>
                    {
                        ["requestUri"] = protocolException.RequestUri?.ToString(),
                    });
                return true;

            case OperationCanceledException when !httpContext.RequestAborted.IsCancellationRequested:
                await WriteProblemAsync(
                    httpContext,
                    StatusCodes.Status504GatewayTimeout,
                    "Blizzard API request timed out",
                    exception.Message,
                    null);
                return true;

            case HttpRequestException:
                await WriteProblemAsync(
                    httpContext,
                    StatusCodes.Status502BadGateway,
                    "Blizzard API is unavailable",
                    "The upstream Blizzard API could not be reached.",
                    null);
                return true;

            default:
                return false;
        }
    }

    private async Task WriteProblemAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        IDictionary<string, object?>? extensions,
        TimeSpan? retryAfter = null)
    {
        logger.LogWarning("Upstream Blizzard request failed: {Title} - {Detail}", title, detail);

        if (retryAfter is { } delay)
        {
            var seconds = Math.Max(0, (int)Math.Ceiling(delay.TotalSeconds));
            httpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        }

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            extensions: extensions).ExecuteAsync(httpContext);
    }

    private static int MapStatusCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
        HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => StatusCodes.Status502BadGateway,
        HttpStatusCode.TooManyRequests => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status502BadGateway,
    };
}
