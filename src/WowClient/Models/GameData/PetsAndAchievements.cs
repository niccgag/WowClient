using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.GameData;

/// <summary>A battle pet.</summary>
public sealed record Pet
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("battle_pet_type")]
    public WowName? BattlePetType { get; init; }

    [JsonPropertyName("creature")]
    public WowLink? Creature { get; init; }

    [JsonPropertyName("is_capturable")]
    public bool IsCapturable { get; init; }

    [JsonPropertyName("is_tradable")]
    public bool IsTradable { get; init; }

    [JsonPropertyName("is_battlepet")]
    public bool IsBattlePet { get; init; }

    [JsonPropertyName("is_alliance_only")]
    public bool IsAllianceOnly { get; init; }

    [JsonPropertyName("is_horde_only")]
    public bool IsHordeOnly { get; init; }

    [JsonPropertyName("abilities")]
    public List<PetAbility>? Abilities { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>An ability available to a battle pet.</summary>
public sealed record PetAbility
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("slot")]
    public int Slot { get; init; }

    [JsonPropertyName("required_level")]
    public int RequiredLevel { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A compact pet reference.</summary>
public sealed record PetReference
{
    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }
}

/// <summary>The pet index.</summary>
public sealed record PetIndex
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("pets")]
    public List<PetReference>? Pets { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A WoW achievement.</summary>
public sealed record Achievement
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("category")]
    public WowLinkedName? Category { get; init; }

    [JsonPropertyName("points")]
    public int Points { get; init; }

    [JsonPropertyName("is_account_wide")]
    public bool IsAccountWide { get; init; }

    [JsonPropertyName("reward_description")]
    public string? RewardDescription { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>An achievement category reference.</summary>
public sealed record AchievementCategoryReference
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>The achievement category index.</summary>
public sealed record AchievementCategoryIndex
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("categories")]
    public List<AchievementCategoryReference>? Categories { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
