namespace WowClient.Auth;

/// <summary>
/// Supplies OAuth access tokens for API requests.
/// </summary>
public interface IAccessTokenProvider
{
    /// <summary>
    /// Gets a valid access token, acquiring or refreshing it as needed.
    /// </summary>
    ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Discards the cached token so the next request acquires a new one.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="InvalidateToken(string)"/> so an unauthorized response for a stale token
    /// cannot clear a newer token.
    /// </remarks>
    void InvalidateToken()
    {
    }

    /// <summary>
    /// Discards a cached token after an unauthorized response. Implementations should only clear the
    /// cache when <paramref name="token"/> is still the current token.
    /// </summary>
    /// <param name="token">Token that was rejected by the API.</param>
    void InvalidateToken(string token)
    {
        InvalidateToken();
    }
}
