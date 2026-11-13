using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.Profile;

/// <summary>A character's PvP summary.</summary>
public sealed record CharacterPvpSummary
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("character")]
    public CharacterReference? Character { get; init; }

    [JsonPropertyName("honor_level")]
    public int HonorLevel { get; init; }

    [JsonPropertyName("pvp_brackets")]
    public List<PvpBracket>? PvpBrackets { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>PvP rating for a bracket.</summary>
public sealed record PvpBracket
{
    [JsonPropertyName("bracket")]
    public WowName? Bracket { get; init; }

    [JsonPropertyName("rating")]
    public int? Rating { get; init; }

    [JsonPropertyName("season")]
    public int? Season { get; init; }

    [JsonPropertyName("tier")]
    public WowName? Tier { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A character's mythic keystone profile.</summary>
public sealed record MythicKeystoneProfile
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("character")]
    public CharacterReference? Character { get; init; }

    [JsonPropertyName("current_period")]
    public MythicKeystonePeriod? CurrentPeriod { get; init; }

    [JsonPropertyName("current_mythic_rating")]
    public MythicKeystoneRating? CurrentMythicRating { get; init; }

    [JsonPropertyName("seasons")]
    public List<MythicKeystoneSeason>? Seasons { get; init; }

    [JsonPropertyName("best_runs")]
    public List<MythicKeystoneRun>? BestRuns { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A mythic keystone scoring period.</summary>
public sealed record MythicKeystonePeriod
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("start_timestamp")]
    public long StartTimestamp { get; init; }

    [JsonPropertyName("end_timestamp")]
    public long EndTimestamp { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A mythic keystone season reference.</summary>
public sealed record MythicKeystoneSeason
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("start_timestamp")]
    public long? StartTimestamp { get; init; }

    [JsonPropertyName("end_timestamp")]
    public long? EndTimestamp { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A mythic keystone rating with its display color.</summary>
public sealed record MythicKeystoneRating
{
    [JsonPropertyName("rating")]
    public double Rating { get; init; }

    [JsonPropertyName("color")]
    public MythicKeystoneColor? Color { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>RGBA color used for mythic keystone rating displays.</summary>
public sealed record MythicKeystoneColor
{
    [JsonPropertyName("r")]
    public double R { get; init; }

    [JsonPropertyName("g")]
    public double G { get; init; }

    [JsonPropertyName("b")]
    public double B { get; init; }

    [JsonPropertyName("a")]
    public double A { get; init; }
}

/// <summary>A best mythic keystone run.</summary>
public sealed record MythicKeystoneRun
{
    [JsonPropertyName("dungeon")]
    public WowIdName? Dungeon { get; init; }

    [JsonPropertyName("keystone_level")]
    public int KeystoneLevel { get; init; }

    [JsonPropertyName("duration")]
    public long Duration { get; init; }

    [JsonPropertyName("completed_timestamp")]
    public long CompletedTimestamp { get; init; }

    [JsonPropertyName("is_completed_within_time")]
    public bool IsCompletedWithinTime { get; init; }

    [JsonPropertyName("members")]
    public List<MythicKeystoneMember>? Members { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A party member in a mythic keystone run.</summary>
public sealed record MythicKeystoneMember
{
    [JsonPropertyName("character")]
    public CharacterReference? Character { get; init; }

    [JsonPropertyName("specialization")]
    public WowIdName? Specialization { get; init; }

    [JsonPropertyName("race")]
    public WowIdName? Race { get; init; }

    [JsonPropertyName("equipped_item_level")]
    public int? EquippedItemLevel { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
