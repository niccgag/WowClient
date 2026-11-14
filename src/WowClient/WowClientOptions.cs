using System.Reflection;

namespace WowClient;

/// <summary>
/// Configuration for <see cref="WowClient"/>.
/// </summary>
public sealed class WowClientOptions
{
    // HttpClient.Timeout, CancellationTokenSource.CancelAfter, and Task.Delay all reject
    // durations larger than int.MaxValue milliseconds, so fail fast with a clear message instead.
    private static readonly TimeSpan MaxSupportedDuration = TimeSpan.FromMilliseconds(int.MaxValue);

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
    /// Timeout applied to the entire HTTP operation, including response body reads. Defaults to 30 seconds.
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

    internal IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (Timeout <= TimeSpan.Zero)
        {
            errors.Add("WowClientOptions.Timeout must be positive.");
        }
        else if (Timeout > MaxSupportedDuration)
        {
            errors.Add(
                $"WowClientOptions.Timeout must not exceed {MaxSupportedDuration}, " +
                "the maximum accepted by HttpClient and CancellationTokenSource.");
        }

        if (!Enum.IsDefined(Region))
        {
            errors.Add("WowClientOptions.Region is not a supported region.");
        }

        if (string.IsNullOrWhiteSpace(Locale))
        {
            errors.Add("WowClientOptions.Locale must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(UserAgent))
        {
            errors.Add("WowClientOptions.UserAgent must not be empty.");
        }

        if (MaxRateLimitRetries < 0)
        {
            errors.Add("WowClientOptions.MaxRateLimitRetries must be zero or greater.");
        }

        if (MaxRateLimitDelay < TimeSpan.Zero)
        {
            errors.Add("WowClientOptions.MaxRateLimitDelay must be zero or greater.");
        }
        else if (MaxRateLimitDelay > MaxSupportedDuration)
        {
            errors.Add(
                $"WowClientOptions.MaxRateLimitDelay must not exceed {MaxSupportedDuration}, " +
                "the maximum accepted by Task.Delay.");
        }

        ValidateBaseUri(ApiBaseUrl, nameof(ApiBaseUrl), errors);
        ValidateBaseUri(OAuthBaseUrl, nameof(OAuthBaseUrl), errors);

        return errors;
    }

    private static void ValidateBaseUri(Uri? uri, string propertyName, List<string> errors)
    {
        if (uri is null)
        {
            return;
        }

        if (!uri.IsAbsoluteUri)
        {
            errors.Add($"WowClientOptions.{propertyName} must be an absolute URI.");
            return;
        }

        if (uri.Scheme == Uri.UriSchemeHttp)
        {
            // Plain http would send the client secret and bearer tokens in plaintext; allow it only
            // for loopback addresses (local mocks and development servers).
            if (!uri.IsLoopback)
            {
                errors.Add(
                    $"WowClientOptions.{propertyName} must use https; " +
                    "plain http is allowed only for loopback addresses.");
            }
        }
        else if (uri.Scheme != Uri.UriSchemeHttps)
        {
            errors.Add($"WowClientOptions.{propertyName} must use https.");
        }

        // The base URI is concatenated with the request path, so a query string or fragment would
        // swallow the path (https://host/base?x=1 + /data/... => /base), and user info would leak
        // credentials into request URIs and logs.
        if (!string.IsNullOrEmpty(uri.Query))
        {
            errors.Add($"WowClientOptions.{propertyName} must not include a query string.");
        }

        if (!string.IsNullOrEmpty(uri.Fragment))
        {
            errors.Add($"WowClientOptions.{propertyName} must not include a fragment.");
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            errors.Add($"WowClientOptions.{propertyName} must not include user info.");
        }
    }
}

internal static class WowClientDefaults
{
    public const string ApiHttpClientName = "WowClient.Api";
    public const string OAuthHttpClientName = "WowClient.OAuth";

    public static readonly string UserAgent = $"WowClient/{GetVersion()}";

    public static readonly TimeSpan TokenRefreshSkew = TimeSpan.FromSeconds(60);

    private static string GetVersion()
    {
        var assembly = typeof(WowClientOptions).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            var metadataIndex = informationalVersion.IndexOf('+');
            return metadataIndex >= 0 ? informationalVersion[..metadataIndex] : informationalVersion;
        }

        return assembly.GetName().Version is { } version
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : "0.0.0";
    }
}
