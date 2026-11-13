using System.Text.Json;
using System.Text.Json.Serialization;
using WowClient.Models.GameData;

namespace WowClient.Models.Profile;

/// <summary>A character's equipped items.</summary>
public sealed record CharacterEquipment
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("character")]
    public CharacterReference? Character { get; init; }

    [JsonPropertyName("equipped_items")]
    public List<EquippedItem>? EquippedItems { get; init; }

    [JsonPropertyName("average_item_level")]
    public int AverageItemLevel { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>One equipped item slot.</summary>
public sealed record EquippedItem
{
    [JsonPropertyName("item")]
    public EquippedItemInfo? Item { get; init; }

    [JsonPropertyName("slot")]
    public WowName? Slot { get; init; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    [JsonPropertyName("context")]
    public int? Context { get; init; }

    [JsonPropertyName("bonus_list")]
    public List<int>? BonusList { get; init; }

    [JsonPropertyName("quality")]
    public WowName? Quality { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Identity of an equipped item.</summary>
public sealed record EquippedItemInfo
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A character's mount collection.</summary>
public sealed record CharacterMounts
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("character")]
    public CharacterReference? Character { get; init; }

    [JsonPropertyName("mounts")]
    public List<CharacterMount>? Mounts { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>One mount in a character's collection.</summary>
public sealed record CharacterMount
{
    [JsonPropertyName("mount")]
    public MountReference? Mount { get; init; }

    [JsonPropertyName("is_useable")]
    public bool IsUseable { get; init; }

    [JsonPropertyName("is_favorite")]
    public bool IsFavorite { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A character's achievements.</summary>
public sealed record CharacterAchievements
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("character")]
    public CharacterReference? Character { get; init; }

    [JsonPropertyName("total_quantity")]
    public int TotalQuantity { get; init; }

    [JsonPropertyName("total_points")]
    public int TotalPoints { get; init; }

    [JsonPropertyName("achievements")]
    public List<CharacterAchievement>? Achievements { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>An achievement earned by a character.</summary>
public sealed record CharacterAchievement
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("achievement")]
    public AchievementReference? Achievement { get; init; }

    [JsonPropertyName("criteria")]
    public CharacterAchievementCriteria? Criteria { get; init; }

    [JsonPropertyName("completed_timestamp")]
    public long? CompletedTimestamp { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>An achievement reference by id and name.</summary>
public sealed record AchievementReference
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>Achievement criteria progress.</summary>
public sealed record CharacterAchievementCriteria
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("amount")]
    public int? Amount { get; init; }

    [JsonPropertyName("is_completed")]
    public bool? IsCompleted { get; init; }

    [JsonPropertyName("child_criteria")]
    public List<CharacterAchievementCriteria>? ChildCriteria { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
