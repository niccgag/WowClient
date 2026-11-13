using System.Text.Json.Serialization;

namespace WowClient.Json;

/// <summary>Blizzard API error payload.</summary>
internal sealed class WowErrorResponse
{
    [JsonPropertyName("code")]
    public int? Code { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }
}

/// <summary>Battle.net OAuth error payload.</summary>
internal sealed class OAuthErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
}
