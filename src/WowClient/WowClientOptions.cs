namespace WowClient;

/// <summary>
/// Configuration for <see cref="WowClient"/>.
/// </summary>
public sealed class WowClientOptions
{
    /// <summary>
    /// Battle.net API client ID from <see href="https://develop.battle.net/access/clients"/>.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Battle.net API client secret. Never hardcode this value in source control.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Region used for API hosts and default namespaces. Defaults to <see cref="WowRegion.Us"/>.
    /// </summary>
    public WowRegion Region { get; set; } = WowRegion.Us;

    /// <summary>
    /// Default locale query value. Defaults to <see cref="WowLocale.EnglishUs"/>.
    /// </summary>
    public string Locale { get; set; } = WowLocale.EnglishUs;

    /// <summary>
    /// Optional API base URL override. When <see langword="null"/> the region host
    /// (<c>https://{region}.api.blizzard.com</c>) is used.
    /// </summary>
    public Uri? ApiBaseUrl { get; set; }

    /// <summary>
    /// Optional OAuth base URL override. When <see langword="null"/>,
    /// <c>https://oauth.battle.net</c> is used.
    /// </summary>
    public Uri? OAuthBaseUrl { get; set; }

    /// <summary>
    /// Timeout applied to HTTP requests created by the library. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// User-Agent header sent with API requests.
    /// </summary>
    public string UserAgent { get; set; } = WowClientDefaults.UserAgent;

    /// <summary>
    /// Number of times a request rejected with HTTP 429 will be retried. Defaults to 1.
    /// </summary>
    public int MaxRateLimitRetries { get; set; } = 1;

    /// <summary>
    /// Longest <c>Retry-After</c> delay the client is willing to wait before surfacing the error.
    /// Defaults to 5 seconds.
    /// </summary>
    public TimeSpan MaxRateLimitDelay { get; set; } = TimeSpan.FromSeconds(5);

    internal Uri ResolveApiBaseUri() =>
        ApiBaseUrl ?? new Uri($"https://{Region.ToApiHost()}", UriKind.Absolute);

    internal Uri ResolveOAuthBaseUri() =>
        OAuthBaseUrl ?? new Uri("https://oauth.battle.net", UriKind.Absolute);
}

internal static class WowClientDefaults
{
    public const string ApiHttpClientName = "WowClient.Api";
    public const string OAuthHttpClientName = "WowClient.OAuth";
    public const string UserAgent = "WowClient/0.1.0";

    public static readonly TimeSpan TokenRefreshSkew = TimeSpan.FromSeconds(60);
}
