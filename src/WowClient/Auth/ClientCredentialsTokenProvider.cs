using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WowClient.Exceptions;
using WowClient.Json;

namespace WowClient.Auth;

/// <summary>
/// Acquires and caches tokens using the OAuth client credentials flow.
/// </summary>
internal sealed class ClientCredentialsTokenProvider : IAccessTokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WowClientOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private AccessToken? _current;

    public ClientCredentialsTokenProvider(
        IHttpClientFactory httpClientFactory,
        WowClientOptions options,
        TimeProvider timeProvider)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = Volatile.Read(ref _current);
        if (token is not null && token.ExpiresAt > _timeProvider.GetUtcNow())
        {
            return token.Value;
        }

        return await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    public void InvalidateToken() => Volatile.Write(ref _current, null);

    private async Task<string> RefreshAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var token = Volatile.Read(ref _current);
            if (token is not null && token.ExpiresAt > _timeProvider.GetUtcNow())
            {
                return token.Value;
            }

            if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
            {
                throw new WowAuthenticationException(
                    "Blizzard client credentials are not configured. Set WowClientOptions.ClientId and WowClientOptions.ClientSecret.");
            }

            var refreshed = await RequestTokenAsync(cancellationToken).ConfigureAwait(false);
            Volatile.Write(ref _current, refreshed);
            return refreshed.Value;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<AccessToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(WowClientDefaults.OAuthHttpClientName);
        var tokenEndpoint = new Uri(
            _options.ResolveOAuthBaseUri().AbsoluteUri.TrimEnd('/') + "/token",
            UriKind.Absolute);

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", _options.UserAgent);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
        });

        using var response = await client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var (error, description) = ParseOAuthError(body);
            var reason = string.IsNullOrWhiteSpace(response.ReasonPhrase)
                ? response.StatusCode.ToString()
                : response.ReasonPhrase;
            throw new WowAuthenticationException(
                $"Blizzard OAuth token request failed with status {(int)response.StatusCode} ({reason}).",
                response.StatusCode,
                error,
                description);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var token = await JsonSerializer
            .DeserializeAsync(stream, WowJsonSerializerContext.Default.TokenResponse, cancellationToken)
            .ConfigureAwait(false);

        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new WowAuthenticationException("Blizzard OAuth token response did not contain an access token.");
        }

        var lifetime = TimeSpan.FromSeconds(Math.Max(1, token.ExpiresIn));
        var skew = TimeSpan.FromSeconds(
            Math.Min(WowClientDefaults.TokenRefreshSkew.TotalSeconds, lifetime.TotalSeconds / 2d));

        return new AccessToken(token.AccessToken, _timeProvider.GetUtcNow() + lifetime - skew);
    }

    private static (string? Error, string? Description) ParseOAuthError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, null);
        }

        try
        {
            var error = JsonSerializer.Deserialize(body, WowJsonSerializerContext.Default.OAuthErrorResponse);
            return (error?.Error, error?.ErrorDescription);
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }
}
