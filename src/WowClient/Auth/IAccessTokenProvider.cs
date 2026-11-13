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
    void InvalidateToken()
    {
    }
}
