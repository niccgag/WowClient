using System.Text.Json;
using System.Text.Json.Serialization;

namespace WowClient.Models.GameData;

/// <summary>A page of results from a Blizzard search endpoint.</summary>
/// <typeparam name="T">Type of each result document.</typeparam>
public sealed record WowSearchResponse<T>
{
    /// <summary>One-based page number returned by the API.</summary>
    [JsonPropertyName("page")]
    public int Page { get; init; }

    /// <summary>Number of results requested for each page.</summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    /// <summary>
    /// Largest page size the API reported as accepted for this query. The value is query-specific
    /// and distinct from the client-side cap of 100 enforced by <c>SearchItemsAsync</c>.
    /// </summary>
    [JsonPropertyName("maxPageSize")]
    public int MaxPageSize { get; init; }

    /// <summary>Total number of pages available for the query.</summary>
    [JsonPropertyName("pageCount")]
    public int PageCount { get; init; }

    /// <summary>
    /// Whether the endpoint stopped counting at its configured limit instead of returning an exact total.
    /// </summary>
    [JsonPropertyName("resultCountCapped")]
    public bool ResultCountCapped { get; init; }

    /// <summary>Results for this page.</summary>
    [JsonPropertyName("results")]
    public List<WowSearchResult<T>>? Results { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A single search result with its source document link.</summary>
/// <typeparam name="T">Type of the result document.</typeparam>
public sealed record WowSearchResult<T>
{
    [JsonPropertyName("key")]
    public WowLink? Key { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }
}

/// <summary>
/// An item returned by the search endpoint. Search results use localized name maps instead of the
/// single localized string returned by the item detail endpoint.
/// </summary>
public sealed record ItemSearchResult
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public WowLocalizedName? Name { get; init; }

    [JsonPropertyName("quality")]
    public WowSearchName? Quality { get; init; }

    [JsonPropertyName("level")]
    public int Level { get; init; }

    [JsonPropertyName("required_level")]
    public int RequiredLevel { get; init; }

    [JsonPropertyName("item_class")]
    public WowSearchName? ItemClass { get; init; }

    [JsonPropertyName("item_subclass")]
    public WowSearchName? ItemSubclass { get; init; }

    [JsonPropertyName("inventory_type")]
    public WowSearchName? InventoryType { get; init; }

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

/// <summary>A type/name pair in which the name is a localized string map.</summary>
public sealed record WowSearchName
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("name")]
    public WowLocalizedName? Name { get; init; }
}
