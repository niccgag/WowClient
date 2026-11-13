using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.GameData;

/// <summary>A page of results from a Blizzard search endpoint.</summary>
/// <typeparam name="T">Type of each result document.</typeparam>
public sealed record WowSearchResponse<T>
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; init; }

    [JsonPropertyName("max_page")]
    public int MaxPage { get; init; }

    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }

    [JsonPropertyName("results")]
    public List<WowSearchResult<T>>? Results { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A single search result with its source document link.</summary>
/// <typeparam name="T">Type of the result document.</typeparam>
public sealed record WowSearchResult<T>
{
    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }
}
