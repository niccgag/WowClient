using System.Globalization;
using System.Runtime.CompilerServices;
using WowClient.Http;
using WowClient.Models;
using WowClient.Models.GameData;

namespace WowClient.Endpoints;

internal sealed class GameDataClient : IGameDataClient
{
    // Client-side page size cap, independent of the query-specific maxPageSize the API reports.
    private const int MaxSearchPageSize = 100;

    private readonly WowApiRequestExecutor _executor;
    private readonly WowClientOptions _options;

    public GameDataClient(WowApiRequestExecutor executor, WowClientOptions options)
    {
        _executor = executor;
        _options = options;
    }

    private string StaticNamespace => WowNamespaces.Static(_options.Region);

    private string DynamicNamespace => WowNamespaces.Dynamic(_options.Region);

    public Task<RealmIndex> GetRealmIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetDynamicAsync<RealmIndex>("/data/wow/realm/index", requestOptions, cancellationToken);

    public Task<Realm> GetRealmAsync(
        string realmSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(realmSlug);

        return GetDynamicAsync<Realm>(
            $"/data/wow/realm/{Uri.EscapeDataString(realmSlug)}",
            requestOptions,
            cancellationToken);
    }

    public Task<ConnectedRealm> GetConnectedRealmAsync(
        int connectedRealmId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetDynamicAsync<ConnectedRealm>(
            $"/data/wow/connected-realm/{connectedRealmId}",
            requestOptions,
            cancellationToken);

    public Task<Item> GetItemAsync(
        int itemId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<Item>($"/data/wow/item/{itemId}", requestOptions, cancellationToken);

    public Task<WowMedia> GetItemMediaAsync(
        int itemId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<WowMedia>($"/data/wow/media/item/{itemId}", requestOptions, cancellationToken);

    public Task<WowSearchResponse<ItemSearchResult>> SearchItemsAsync(
        string query,
        int page = 1,
        int pageSize = 100,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, MaxSearchPageSize);

        var locale = requestOptions?.Locale ?? _options.Locale;
        var queryParameters = new Dictionary<string, string?>
        {
            [$"name.{locale}"] = query,
            ["orderby"] = "id",
            ["_page"] = page.ToString(CultureInfo.InvariantCulture),
            ["_pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
        };

        return _executor.GetAsync<WowSearchResponse<ItemSearchResult>>(
            "/data/wow/search/item",
            StaticNamespace,
            requestOptions,
            queryParameters,
            cancellationToken);
    }

    public IAsyncEnumerable<WowSearchResult<ItemSearchResult>> SearchAllItemsAsync(
        string query,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        // Validate eagerly: this method is not an iterator, so argument errors surface at the call
        // site instead of on first enumeration.
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        return SearchAllItemsIteratorAsync(query, requestOptions, cancellationToken);
    }

    private async IAsyncEnumerable<WowSearchResult<ItemSearchResult>> SearchAllItemsIteratorAsync(
        string query,
        WowRequestOptions? requestOptions,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var page = 1;
        while (true)
        {
            var response = await SearchItemsAsync(
                    query,
                    page,
                    MaxSearchPageSize,
                    requestOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            if (response.Results is { Count: > 0 })
            {
                foreach (var result in response.Results)
                {
                    yield return result;
                }
            }

            // Follow PageCount even when a page comes back empty; only stop once the reported page
            // count is exhausted (or the API does not report one).
            if (response.PageCount <= 0 || page >= response.PageCount)
            {
                yield break;
            }

            page++;
        }
    }

    public Task<Spell> GetSpellAsync(
        int spellId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<Spell>($"/data/wow/spell/{spellId}", requestOptions, cancellationToken);

    public Task<WowMedia> GetSpellMediaAsync(
        int spellId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<WowMedia>($"/data/wow/media/spell/{spellId}", requestOptions, cancellationToken);

    public Task<MountIndex> GetMountIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<MountIndex>("/data/wow/mount/index", requestOptions, cancellationToken);

    public Task<Mount> GetMountAsync(
        int mountId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<Mount>($"/data/wow/mount/{mountId}", requestOptions, cancellationToken);

    public Task<PetIndex> GetPetIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<PetIndex>("/data/wow/pet/index", requestOptions, cancellationToken);

    public Task<Pet> GetPetAsync(
        int petId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<Pet>($"/data/wow/pet/{petId}", requestOptions, cancellationToken);

    public Task<Achievement> GetAchievementAsync(
        int achievementId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<Achievement>(
            $"/data/wow/achievement/{achievementId}",
            requestOptions,
            cancellationToken);

    public Task<WowMedia> GetAchievementMediaAsync(
        int achievementId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<WowMedia>(
            $"/data/wow/media/achievement/{achievementId}",
            requestOptions,
            cancellationToken);

    public Task<AchievementCategoryIndex> GetAchievementCategoryIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<AchievementCategoryIndex>(
            "/data/wow/achievement-category/index",
            requestOptions,
            cancellationToken);

    public Task<ItemClassIndex> GetItemClassIndexAsync(
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<ItemClassIndex>(
            "/data/wow/item-class/index",
            requestOptions,
            cancellationToken);

    public Task<ItemClass> GetItemClassAsync(
        int itemClassId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<ItemClass>(
            $"/data/wow/item-class/{itemClassId}",
            requestOptions,
            cancellationToken);

    public Task<ItemSubclass> GetItemSubclassAsync(
        int itemClassId,
        int itemSubclassId,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetStaticAsync<ItemSubclass>(
            $"/data/wow/item-class/{itemClassId}/item-subclass/{itemSubclassId}",
            requestOptions,
            cancellationToken);

    private Task<T> GetStaticAsync<T>(
        string path,
        WowRequestOptions? requestOptions,
        CancellationToken cancellationToken) =>
        _executor.GetAsync<T>(path, StaticNamespace, requestOptions, null, cancellationToken);

    private Task<T> GetDynamicAsync<T>(
        string path,
        WowRequestOptions? requestOptions,
        CancellationToken cancellationToken) =>
        _executor.GetAsync<T>(path, DynamicNamespace, requestOptions, null, cancellationToken);
}
