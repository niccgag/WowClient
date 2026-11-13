using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.Profile;

/// <summary>A character's primary and secondary professions.</summary>
public sealed record CharacterProfessions
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("character")]
    public CharacterReference? Character { get; init; }

    [JsonPropertyName("primaries")]
    public List<Profession>? Primaries { get; init; }

    [JsonPropertyName("secondaries")]
    public List<Profession>? Secondaries { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>One profession known by a character.</summary>
public sealed record Profession
{
    [JsonPropertyName("name")]
    public WowName? Name { get; init; }

    [JsonPropertyName("profession")]
    public WowIdName? ProfessionInfo { get; init; }

    [JsonPropertyName("tiers")]
    public List<ProfessionTier>? Tiers { get; init; }

    [JsonPropertyName("skill_points")]
    public int? SkillPoints { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A profession expansion tier.</summary>
public sealed record ProfessionTier
{
    [JsonPropertyName("tier")]
    public WowIdName? Tier { get; init; }

    [JsonPropertyName("skill_points")]
    public int? SkillPoints { get; init; }

    [JsonPropertyName("max_skill_points")]
    public int? MaxSkillPoints { get; init; }

    [JsonPropertyName("known_recipes")]
    public List<ProfessionRecipe>? KnownRecipes { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A recipe known by a character.</summary>
public sealed record ProfessionRecipe
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
