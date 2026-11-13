using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.GameData;

/// <summary>A WoW spell.</summary>
public sealed record Spell
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A WoW mount.</summary>
public sealed record Mount
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("creature_display")]
    public WowLink? CreatureDisplay { get; init; }

    [JsonPropertyName("faction")]
    public WowName? Faction { get; init; }

    [JsonPropertyName("source")]
    public WowName? Source { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A compact mount reference.</summary>
public sealed record MountReference
{
    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }
}

/// <summary>The mount index.</summary>
public sealed record MountIndex
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("mounts")]
    public List<MountReference>? Mounts { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
