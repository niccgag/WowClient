using System.Text.Json;
using WowClient.Auth;
using WowClient.Endpoints;
using WowClient.Http;

namespace WowClient;

/// <summary>
/// Default <see cref="IWowClient"/> implementation. Use the
/// <see cref="Extensions.ServiceCollectionExtensions.AddWowClient"/> DI extension or
/// <see cref="Create"/> for manual construction.
/// </summary>
public sealed class WowClient : IWowClient, IDisposable
{
    private readonly WowApiRequestExecutor _executor;
    private readonly WowClientOptions _options;
    private readonly HttpClient? _ownedApiClient;
    private readonly HttpClient? _ownedAuthClient;
    private int _disposed;

    internal WowClient(
        IGameDataClient gameData,
        IProfileClient profile,
        WowApiRequestExecutor executor,
        WowClientOptions options,
        HttpClient? ownedApiClient = null,
        HttpClient? ownedAuthClient = null)
    {
        GameData = gameData;
        Profile = profile;
        _executor = executor;
        _options = options;
        _ownedApiClient = ownedApiClient;
        _ownedAuthClient = ownedAuthClient;
    }

    /// <inheritdoc />
    public IGameDataClient GameData { get; }

    /// <inheritdoc />
    public IProfileClient Profile { get; }

    /// <summary>
    /// Creates a client without a dependency injection container.
    /// </summary>
    /// <param name="options">Client configuration, including region and credentials.</param>
    /// <param name="apiClient">
    /// Optional HTTP client used for API calls. When omitted, the client creates one and applies
    /// <see cref="WowClientOptions.Timeout"/>. A caller-provided client remains owned by the caller.
    /// </param>
    /// <param name="authClient">
    /// Optional HTTP client used for OAuth token requests. When omitted, the client creates one and applies
    /// <see cref="WowClientOptions.Timeout"/>. A caller-provided client remains owned by the caller.
    /// </param>
    /// <param name="timeProvider">Optional time source, primarily useful for testing token expiry.</param>
    /// <returns>A configured <see cref="WowClient"/>.</returns>
    public static WowClient Create(
        WowClientOptions options,
        HttpClient? apiClient = null,
        HttpClient? authClient = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "WowClientOptions.Timeout must be positive.");
        }

        var validationErrors = options.Validate();
        if (validationErrors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", validationErrors), nameof(options));
        }

        var ownsApiClient = apiClient is null;
        var ownsAuthClient = authClient is null;

        apiClient ??= new HttpClient { Timeout = options.Timeout };
        authClient ??= new HttpClient { Timeout = options.Timeout };
        timeProvider ??= TimeProvider.System;

        var tokenProvider = new ClientCredentialsTokenProvider(
            new SingleHttpClientFactory(authClient),
            options,
            timeProvider);

        var executor = new WowApiRequestExecutor(
            new SingleHttpClientFactory(apiClient),
            tokenProvider,
            options,
            timeProvider);

        return new WowClient(
            new GameDataClient(executor, options),
            new ProfileClient(executor, options),
            executor,
            options,
            ownsApiClient ? apiClient : null,
            ownsAuthClient ? authClient : null);
    }

    /// <inheritdoc />
    public Task<JsonDocument> GetRawAsync(
        string relativePath,
        WowRequestOptions? requestOptions = null,
        IReadOnlyDictionary<string, string?>? queryParameters = null,
        CancellationToken cancellationToken = default) =>
        _executor.GetRawAsync(
            relativePath,
            WowNamespaces.ForRawPath(relativePath, _options.Region),
            requestOptions,
            queryParameters,
            cancellationToken);

    /// <summary>
    /// Disposes HTTP clients created internally by <see cref="Create"/>. Caller-provided clients are
    /// left untouched, and clients resolved through dependency injection are managed by its container.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        if (ReferenceEquals(_ownedApiClient, _ownedAuthClient))
        {
            _ownedApiClient?.Dispose();
            return;
        }

        _ownedApiClient?.Dispose();
        _ownedAuthClient?.Dispose();
    }
}
