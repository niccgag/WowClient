using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models;

/// <summary>A hyperlink object returned by the API.</summary>
public sealed record WowLink
{
    [JsonPropertyName("href")]
    public string? Href { get; init; }

    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("id")]
    public int? Id { get; init; }

    /// <summary>
    /// Gets the link target, accepting both the <c>href</c> and <c>key.href</c> shapes used by Blizzard.
    /// </summary>
    [JsonIgnore]
    public string? Url => Href ?? Key?.Href;
}

/// <summary>The <c>_links</c> object returned by the API.</summary>
public sealed record WowLinks
{
    [JsonPropertyName("self")]
    public WowLink? Self { get; init; }
}

/// <summary>A Blizzard type/name pair, for example an item quality.</summary>
public sealed record WowName
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>An id/name pair.</summary>
public sealed record WowIdName
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>A key/name pair.</summary>
public sealed record WowLinkedName
{
    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>A media asset such as an icon URL.</summary>
public sealed record WowAsset
{
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

/// <summary>A media document containing zero or more assets.</summary>
public sealed record WowMedia
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("assets")]
    public List<WowAsset>? Assets { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
