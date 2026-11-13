using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WowClient.Auth;
using WowClient.Exceptions;
using WowClient.Json;

namespace WowClient.Http;

internal sealed class WowApiRequestExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly WowClientOptions _options;

    public WowApiRequestExecutor(
        IHttpClientFactory httpClientFactory,
        IAccessTokenProvider accessTokenProvider,
        WowClientOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _accessTokenProvider = accessTokenProvider;
        _options = options;
    }

    public async Task<T> GetAsync<T>(
        string relativePath,
        string defaultNamespace,
        WowRequestOptions? requestOptions,
        IReadOnlyDictionary<string, string?>? queryParameters,
        CancellationToken cancellationToken)
    {
        var uri = BuildUri(relativePath, defaultNamespace, requestOptions, queryParameters);

        using var response = await SendAsync(uri, cancellationToken).ConfigureAwait(false);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var typeInfo = WowJsonSerializerContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
            ?? throw new NotSupportedException(
                $"Type '{typeof(T)}' is not registered in {nameof(WowJsonSerializerContext)}.");

        var result = await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false);
        return result ?? throw new WowApiException(
            response.StatusCode,
            null,
            null,
            "The API returned an empty response body.",
            uri,
            null);
    }

    public async Task<JsonDocument> GetRawAsync(
        string relativePath,
        string defaultNamespace,
        WowRequestOptions? requestOptions,
        IReadOnlyDictionary<string, string?>? queryParameters,
        CancellationToken cancellationToken)
    {
        var uri = BuildUri(relativePath, defaultNamespace, requestOptions, queryParameters);

        using var response = await SendAsync(uri, cancellationToken).ConfigureAwait(false);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendAsync(Uri uri, CancellationToken cancellationToken)
    {
        var authRetried = false;
        var rateLimitRetries = 0;

        while (true)
        {
            var token = await _accessTokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.TryAddWithoutValidation("User-Agent", _options.UserAgent);

            var client = _httpClientFactory.CreateClient(WowClientDefaults.ApiHttpClientName);
            var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !authRetried)
            {
                authRetried = true;
                response.Dispose();
                _accessTokenProvider.InvalidateToken();
                continue;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                rateLimitRetries < _options.MaxRateLimitRetries)
            {
                var delay = GetRetryDelay(response, _options.MaxRateLimitDelay);
                if (delay is not null)
                {
                    rateLimitRetries++;
                    response.Dispose();
                    if (delay.Value > TimeSpan.Zero)
                    {
                        await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);
                    }

                    continue;
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception = await CreateApiExceptionAsync(response, uri, cancellationToken).ConfigureAwait(false);
                response.Dispose();
                throw exception;
            }

            return response;
        }
    }

    private static TimeSpan? GetRetryDelay(HttpResponseMessage response, TimeSpan maxDelay)
    {
        if (response.Headers.RetryAfter is not { } retryAfter)
        {
            return null;
        }

        var delay = retryAfter.Delta ?? (retryAfter.Date - DateTimeOffset.UtcNow);
        if (delay is not { } value)
        {
            return null;
        }

        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        return value <= maxDelay ? value : null;
    }

    private static async Task<WowApiException> CreateApiExceptionAsync(
        HttpResponseMessage response,
        Uri uri,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        WowErrorResponse? error = null;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                error = JsonSerializer.Deserialize(body, WowJsonSerializerContext.Default.WowErrorResponse);
            }
            catch (JsonException)
            {
                // Non-JSON error responses are surfaced through ResponseBody only.
            }
        }

        return new WowApiException(
            response.StatusCode,
            error?.Code,
            error?.Type,
            error?.Detail,
            uri,
            body);
    }

    private Uri BuildUri(
        string relativePath,
        string defaultNamespace,
        WowRequestOptions? requestOptions,
        IReadOnlyDictionary<string, string?>? queryParameters)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("A request path is required.", nameof(relativePath));
        }

        var path = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        var baseUri = _options.ResolveApiBaseUri();
        var absolute = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + path, UriKind.Absolute);

        var query = new List<string>(4)
        {
            $"namespace={Uri.EscapeDataString(requestOptions?.Namespace ?? defaultNamespace)}",
            $"locale={Uri.EscapeDataString(requestOptions?.Locale ?? _options.Locale)}",
        };

        if (queryParameters is not null)
        {
            foreach (var parameter in queryParameters)
            {
                if (parameter.Value is null)
                {
                    continue;
                }

                query.Add(
                    $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}");
            }
        }

        var builder = new UriBuilder(absolute)
        {
            Query = string.Join("&", query),
        };

        return builder.Uri;
    }
}
