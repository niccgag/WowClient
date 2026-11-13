using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.GameData;

/// <summary>A WoW item.</summary>
public sealed record Item
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("quality")]
    public WowName? Quality { get; init; }

    [JsonPropertyName("level")]
    public int Level { get; init; }

    [JsonPropertyName("required_level")]
    public int RequiredLevel { get; init; }

    [JsonPropertyName("media")]
    public WowLink? Media { get; init; }

    [JsonPropertyName("item_class")]
    public WowName? ItemClass { get; init; }

    [JsonPropertyName("item_subclass")]
    public WowName? ItemSubclass { get; init; }

    [JsonPropertyName("inventory_type")]
    public WowName? InventoryType { get; init; }

    [JsonPropertyName("purchase_price")]
    public long PurchasePrice { get; init; }

    [JsonPropertyName("sell_price")]
    public long SellPrice { get; init; }

    [JsonPropertyName("max_count")]
    public int MaxCount { get; init; }

    [JsonPropertyName("is_equippable")]
    public bool IsEquippable { get; init; }

    [JsonPropertyName("is_stackable")]
    public bool IsStackable { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>The item class index.</summary>
public sealed record ItemClassIndex
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("classes")]
    public List<ItemClassReference>? Classes { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A compact item class reference.</summary>
public sealed record ItemClassReference
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>An item class and its subclasses.</summary>
public sealed record ItemClass
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("item_subclasses")]
    public List<ItemSubclassReference>? ItemSubclasses { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A compact item subclass reference.</summary>
public sealed record ItemSubclassReference
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>An item subclass within a class.</summary>
public sealed record ItemSubclass
{
    [JsonPropertyName("_links")]
    public WowLinks? Links { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("class_id")]
    public int ClassId { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
