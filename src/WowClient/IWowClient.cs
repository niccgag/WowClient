using System.Text.Json;
using WowClient.Endpoints;

namespace WowClient;

/// <summary>
/// A client for the Blizzard World of Warcraft Game Data and Profile APIs.
/// </summary>
public interface IWowClient
{
    /// <summary>Game Data API endpoints (items, spells, realms, and so on).</summary>
    IGameDataClient GameData { get; }

    /// <summary>Profile API endpoints (characters and guilds).</summary>
    IProfileClient Profile { get; }

    /// <summary>
    /// Sends a raw GET request to any path on the configured API host and returns the JSON document.
    /// The namespace is inferred from the path unless <see cref="WowRequestOptions.Namespace"/> is set.
    /// The caller owns the returned <see cref="JsonDocument"/> and must dispose it.
    /// </summary>
    /// <param name="relativePath">
    /// API path, for example <c>/data/wow/item/19019</c>. Query strings embedded in the path are preserved.
    /// </param>
    /// <param name="requestOptions">Optional namespace and locale overrides.</param>
    /// <param name="queryParameters">Additional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JsonDocument> GetRawAsync(
        string relativePath,
        WowRequestOptions? requestOptions = null,
        IReadOnlyDictionary<string, string?>? queryParameters = null,
        CancellationToken cancellationToken = default);
}
