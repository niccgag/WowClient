using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.GameData;

/// <summary>A WoW realm.</summary>
public sealed record Realm
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("slug")]
    public string? Slug { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; init; }

    [JsonPropertyName("type")]
    public WowName? Type { get; init; }

    [JsonPropertyName("is_tournament")]
    public bool IsTournament { get; init; }

    [JsonPropertyName("connected_realm")]
    public WowLink? ConnectedRealm { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A compact realm reference used by index endpoints.</summary>
public sealed record RealmReference
{
    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("slug")]
    public string? Slug { get; init; }
}

/// <summary>The realm index.</summary>
public sealed record RealmIndex
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("realms")]
    public List<RealmReference>? Realms { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>One or more realms connected through cross-realm play.</summary>
public sealed record ConnectedRealm
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("has_queue")]
    public bool HasQueue { get; init; }

    [JsonPropertyName("status")]
    public WowName? Status { get; init; }

    [JsonPropertyName("population")]
    public WowName? Population { get; init; }

    [JsonPropertyName("realms")]
    public List<RealmReference>? Realms { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
