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
    private readonly TimeProvider _timeProvider;

    public WowApiRequestExecutor(
        IHttpClientFactory httpClientFactory,
        IAccessTokenProvider accessTokenProvider,
        WowClientOptions options,
        TimeProvider timeProvider)
    {
        _httpClientFactory = httpClientFactory;
        _accessTokenProvider = accessTokenProvider;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async Task<T> GetAsync<T>(
        string relativePath,
        string defaultNamespace,
        WowRequestOptions? requestOptions,
        IReadOnlyDictionary<string, string?>? queryParameters,
        CancellationToken cancellationToken)
    {
        // Resolve the metadata before the network call so an unregistered type fails without
        // spending a request.
        var typeInfo = WowJsonSerializerContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
            ?? throw new NotSupportedException(
                $"Type '{typeof(T)}' is not registered in {nameof(WowJsonSerializerContext)}.");

        var uri = BuildUri(relativePath, defaultNamespace, requestOptions, queryParameters);
        var deadline = DateTimeOffset.UtcNow + _options.Timeout;
        using var timeout = CreateTimeoutTokenSource(cancellationToken);
        var operationToken = timeout.Token;

        try
        {
            using var response = await SendAsync(uri, operationToken, deadline).ConfigureAwait(false);
            EnsureResponseHasBody(response, uri);
            await using var stream = await response.Content
                .ReadAsStreamAsync(operationToken)
                .ConfigureAwait(false);

            T? result;
            try
            {
                result = await JsonSerializer
                    .DeserializeAsync(stream, typeInfo, operationToken)
                    .ConfigureAwait(false);
            }
            catch (JsonException exception)
            {
                throw new WowApiException(
                    response.StatusCode,
                    null,
                    null,
                    "The API returned malformed JSON.",
                    uri,
                    null,
                    retryAfter: null,
                    innerException: exception);
            }

            return result ?? throw new WowProtocolException(
                "The API returned an empty response body.",
                uri);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw CreateTimeoutException(exception);
        }
    }

    public async Task<JsonDocument> GetRawAsync(
        string relativePath,
        string defaultNamespace,
        WowRequestOptions? requestOptions,
        IReadOnlyDictionary<string, string?>? queryParameters,
        CancellationToken cancellationToken)
    {
        var uri = BuildUri(relativePath, defaultNamespace, requestOptions, queryParameters);
        var deadline = DateTimeOffset.UtcNow + _options.Timeout;
        using var timeout = CreateTimeoutTokenSource(cancellationToken);
        var operationToken = timeout.Token;

        try
        {
            using var response = await SendAsync(uri, operationToken, deadline).ConfigureAwait(false);
            EnsureResponseHasBody(response, uri);
            await using var stream = await response.Content
                .ReadAsStreamAsync(operationToken)
                .ConfigureAwait(false);

            try
            {
                return await JsonDocument
                    .ParseAsync(stream, cancellationToken: operationToken)
                    .ConfigureAwait(false);
            }
            catch (JsonException exception)
            {
                throw new WowApiException(
                    response.StatusCode,
                    null,
                    null,
                    "The API returned malformed JSON.",
                    uri,
                    null,
                    retryAfter: null,
                    innerException: exception);
            }
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw CreateTimeoutException(exception);
        }
    }

    private CancellationTokenSource CreateTimeoutTokenSource(CancellationToken cancellationToken)
    {
        var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);
        return timeout;
    }

    private static void EnsureResponseHasBody(HttpResponseMessage response, Uri uri)
    {
        if (response.StatusCode == HttpStatusCode.NoContent ||
            response.Content.Headers.ContentLength == 0)
        {
            throw new WowProtocolException("The API returned an empty response body.", uri);
        }
    }

    private TaskCanceledException CreateTimeoutException(OperationCanceledException innerException) =>
        new(
            $"The Blizzard API request timed out after {_options.Timeout}.",
            innerException);

    private async Task<HttpResponseMessage> SendAsync(
        Uri uri,
        CancellationToken cancellationToken,
        DateTimeOffset deadline)
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
                _accessTokenProvider.InvalidateToken(token);
                continue;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                rateLimitRetries < _options.MaxRateLimitRetries)
            {
                var delay = GetRetryDelay(response, _options.MaxRateLimitDelay);

                // Only wait when the delay fits inside the remaining operation timeout, measured on
                // the wall clock because CancellationTokenSource.CancelAfter arms the timeout with
                // the system clock rather than the injected TimeProvider. Waiting anyway would
                // cancel the delay and surface the 429 as a misleading timeout.
                if (delay is not null && delay.Value <= deadline - DateTimeOffset.UtcNow)
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
                try
                {
                    throw await CreateApiExceptionAsync(response, uri, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    response.Dispose();
                }
            }

            return response;
        }
    }

    private TimeSpan? GetRetryDelay(HttpResponseMessage response, TimeSpan maxDelay)
    {
        var delay = GetRetryAfter(response);
        return delay <= maxDelay ? delay : null;
    }

    private TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter is not { } retryAfter)
        {
            return null;
        }

        var delay = retryAfter.Delta ?? (retryAfter.Date - _timeProvider.GetUtcNow());
        if (delay is not { } value)
        {
            return null;
        }

        return value < TimeSpan.Zero ? TimeSpan.Zero : value;
    }

    private async Task<WowApiException> CreateApiExceptionAsync(
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
            body,
            GetRetryAfter(response));
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

        // Start with any query string embedded in the path. UriBuilder.Query would otherwise
        // replace it when the generated parameters are assigned.
        var query = new Dictionary<string, string?>(StringComparer.Ordinal);
        var existingQuery = absolute.Query;
        if (existingQuery.StartsWith('?'))
        {
            existingQuery = existingQuery[1..];
        }

        foreach (var part in existingQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            var key = separator < 0 ? part : part[..separator];
            var value = separator < 0 ? string.Empty : part[(separator + 1)..];
            query[Uri.UnescapeDataString(key)] = Uri.UnescapeDataString(value);
        }

        // Precedence: explicit WowRequestOptions beat caller-supplied queryParameters, which beat
        // values embedded in the path; generated defaults fill whatever is still missing.
        if (queryParameters is not null)
        {
            foreach (var parameter in queryParameters)
            {
                if (parameter.Value is not null)
                {
                    query[parameter.Key] = parameter.Value;
                }
            }
        }

        if (requestOptions?.Namespace is not null)
        {
            query["namespace"] = requestOptions.Namespace;
        }
        else if (!query.ContainsKey("namespace"))
        {
            query["namespace"] = defaultNamespace;
        }

        if (requestOptions?.Locale is not null)
        {
            query["locale"] = requestOptions.Locale;
        }
        else if (!query.ContainsKey("locale"))
        {
            query["locale"] = _options.Locale;
        }

        var builder = new UriBuilder(absolute)
        {
            Query = string.Join(
                "&",
                query.Select(parameter =>
                    $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value ?? string.Empty)}")),
        };

        return builder.Uri;
    }
}
