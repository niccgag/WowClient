using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.Profile;

/// <summary>A guild summary.</summary>
public sealed record Guild
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("faction")]
    public WowName? Faction { get; init; }

    [JsonPropertyName("achievement_points")]
    public int AchievementPoints { get; init; }

    [JsonPropertyName("member_count")]
    public int MemberCount { get; init; }

    [JsonPropertyName("realm")]
    public CharacterRealm? Realm { get; init; }

    [JsonPropertyName("created_timestamp")]
    public long CreatedTimestamp { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Compact guild reference embedded in roster and activity responses.</summary>
public sealed record GuildReference
{
    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("realm")]
    public CharacterRealm? Realm { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A guild roster.</summary>
public sealed record GuildRoster
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("guild")]
    public GuildReference? Guild { get; init; }

    [JsonPropertyName("members")]
    public List<GuildMember>? Members { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A guild roster member.</summary>
public sealed record GuildMember
{
    [JsonPropertyName("character")]
    public GuildMemberCharacter? Character { get; init; }

    [JsonPropertyName("rank")]
    public int Rank { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Character details within a guild roster or activity entry.</summary>
public sealed record GuildMemberCharacter
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("realm")]
    public CharacterRealm? Realm { get; init; }

    [JsonPropertyName("level")]
    public int Level { get; init; }

    [JsonPropertyName("playable_class")]
    public WowIdName? PlayableClass { get; init; }

    [JsonPropertyName("playable_race")]
    public WowIdName? PlayableRace { get; init; }

    [JsonPropertyName("faction")]
    public WowName? Faction { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Recent guild activity.</summary>
public sealed record GuildActivity
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("guild")]
    public GuildReference? Guild { get; init; }

    [JsonPropertyName("activities")]
    public List<GuildActivityEntry>? Activities { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A single guild activity entry.</summary>
public sealed record GuildActivityEntry
{
    [JsonPropertyName("character_achievement")]
    public GuildCharacterAchievement? CharacterAchievement { get; init; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>An achievement earned by a guild member.</summary>
public sealed record GuildCharacterAchievement
{
    [JsonPropertyName("character")]
    public GuildMemberCharacter? Character { get; init; }

    [JsonPropertyName("achievement")]
    public AchievementReference? Achievement { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A guild's achievements.</summary>
public sealed record GuildAchievements
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("guild")]
    public GuildReference? Guild { get; init; }

    [JsonPropertyName("total_quantity")]
    public int TotalQuantity { get; init; }

    [JsonPropertyName("total_points")]
    public int TotalPoints { get; init; }

    [JsonPropertyName("achievements")]
    public List<CharacterAchievement>? Achievements { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
