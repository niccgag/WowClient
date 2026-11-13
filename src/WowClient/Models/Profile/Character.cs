using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.Profile;

/// <summary>Realm identifier attached to a character or guild profile.</summary>
public sealed record CharacterRealm
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("slug")]
    public string? Slug { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>Guild information embedded in a character profile.</summary>
public sealed record CharacterGuild
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("realm")]
    public CharacterRealm? Realm { get; init; }

    [JsonPropertyName("faction")]
    public WowName? Faction { get; init; }
}

/// <summary>Compact character reference embedded in collection responses.</summary>
public sealed record CharacterReference
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("realm")]
    public CharacterRealm? Realm { get; init; }
}

/// <summary>The character profile summary.</summary>
public sealed record CharacterProfile
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("gender")]
    public WowName? Gender { get; init; }

    [JsonPropertyName("faction")]
    public WowName? Faction { get; init; }

    [JsonPropertyName("race")]
    public WowIdName? Race { get; init; }

    [JsonPropertyName("character_class")]
    public WowIdName? CharacterClass { get; init; }

    [JsonPropertyName("active_spec")]
    public WowIdName? ActiveSpec { get; init; }

    [JsonPropertyName("realm")]
    public CharacterRealm? Realm { get; init; }

    [JsonPropertyName("guild")]
    public CharacterGuild? Guild { get; init; }

    [JsonPropertyName("level")]
    public int Level { get; init; }

    [JsonPropertyName("achievement_points")]
    public int AchievementPoints { get; init; }

    [JsonPropertyName("achievements")]
    public CharacterAchievementSummary? Achievements { get; init; }

    [JsonPropertyName("last_login_timestamp")]
    public long LastLoginTimestamp { get; init; }

    [JsonPropertyName("average_item_level")]
    public int AverageItemLevel { get; init; }

    [JsonPropertyName("equipped_item_level")]
    public int EquippedItemLevel { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Achievement totals embedded in a character profile summary.</summary>
public sealed record CharacterAchievementSummary
{
    [JsonPropertyName("total_quantity")]
    public int TotalQuantity { get; init; }

    [JsonPropertyName("total_points")]
    public int TotalPoints { get; init; }
}

/// <summary>Whether a character profile is available for the current season.</summary>
public sealed record CharacterStatus
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
