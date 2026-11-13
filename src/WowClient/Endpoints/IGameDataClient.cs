using WowClient.Models;
using WowClient.Models.GameData;

namespace WowClient.Endpoints;

/// <summary>
/// Access to the WoW Game Data API.
/// </summary>
public interface IGameDataClient
{
    /// <summary>Gets every realm.</summary>
    Task<RealmIndex> GetRealmIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a realm by slug, such as <c>tarren-mill</c>.</summary>
    Task<Realm> GetRealmAsync(
        string realmSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a connected realm by ID.</summary>
    Task<ConnectedRealm> GetConnectedRealmAsync(
        int connectedRealmId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an item by ID.</summary>
    Task<Item> GetItemAsync(
        int itemId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the media assets for an item.</summary>
    Task<WowMedia> GetItemMediaAsync(
        int itemId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches for items by name.</summary>
    Task<WowSearchResponse<Item>> SearchItemsAsync(
        string query,
        int page = 1,
        int pageSize = 100,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enumerates every item matching a search query, following pages automatically.</summary>
    IAsyncEnumerable<WowSearchResult<Item>> SearchAllItemsAsync(
        string query,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a spell by ID.</summary>
    Task<Spell> GetSpellAsync(
        int spellId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the media assets for a spell.</summary>
    Task<WowMedia> GetSpellMediaAsync(
        int spellId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets every mount.</summary>
    Task<MountIndex> GetMountIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a mount by ID.</summary>
    Task<Mount> GetMountAsync(
        int mountId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets every battle pet.</summary>
    Task<PetIndex> GetPetIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a battle pet by ID.</summary>
    Task<Pet> GetPetAsync(
        int petId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an achievement by ID.</summary>
    Task<Achievement> GetAchievementAsync(
        int achievementId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the media assets for an achievement.</summary>
    Task<WowMedia> GetAchievementMediaAsync(
        int achievementId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets every achievement category.</summary>
    Task<AchievementCategoryIndex> GetAchievementCategoryIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets every item class.</summary>
    Task<ItemClassIndex> GetItemClassIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an item class and its subclasses.</summary>
    Task<ItemClass> GetItemClassAsync(
        int itemClassId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one item subclass within a class.</summary>
    Task<ItemSubclass> GetItemSubclassAsync(
        int itemClassId,
        int itemSubclassId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);
}
