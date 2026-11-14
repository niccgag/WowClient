using System.Text.Json;
using WowClient.Models.GameData;
using WowClient.Tests.Helpers;

namespace WowClient.Tests;

public sealed class RequestBuildingTests
{
    [Fact]
    public async Task ItemRequestUsesStaticNamespaceAndDefaultLocale()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler);

        await client.GameData.GetItemAsync(19019);

        var request = handler.ApiRequest();
        Assert.Equal("https://api.test/data/wow/item/19019", request.Uri.GetLeftPart(UriPartial.Path));
        Assert.Equal("static-us", request.Uri.QueryValue("namespace"));
        Assert.Equal("en_US", request.Uri.QueryValue("locale"));
        Assert.Equal("WowClient/0.1.0", request.Headers["User-Agent"]);
    }

    [Fact]
    public async Task RealmIndexUsesDynamicNamespace()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("realm-index.json"));
        var client = TestClient.Create(handler);

        await client.GameData.GetRealmIndexAsync();

        Assert.Equal("dynamic-us", handler.ApiRequest().Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task CharacterProfileUsesProfileNamespaceAndEscapesPathSegments()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("character-profile.json"));
        var client = TestClient.Create(handler);

        await client.Profile.GetCharacterProfileAsync("tarren-mill", "Fëa");

        var request = handler.ApiRequest();
        Assert.Equal(
            "/profile/wow/character/tarren-mill/F%C3%ABa",
            request.Uri.AbsolutePath);
        Assert.Equal("profile-us", request.Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task GuildRequestUsesProfileNamespace()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("guild-roster.json"));
        var client = TestClient.Create(handler);

        await client.Profile.GetGuildRosterAsync("tarren-mill", "example-guild");

        var request = handler.ApiRequest();
        Assert.Equal(
            "/data/wow/guild/tarren-mill/example-guild/roster",
            request.Uri.AbsolutePath);
        Assert.Equal("profile-us", request.Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task RequestOptionsOverrideNamespaceAndLocale()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler);

        await client.GameData.GetItemAsync(
            19019,
            new WowRequestOptions
            {
                Namespace = "static-classic-us",
                Locale = WowLocale.GermanDe,
            });

        var request = handler.ApiRequest();
        Assert.Equal("static-classic-us", request.Uri.QueryValue("namespace"));
        Assert.Equal("de_DE", request.Uri.QueryValue("locale"));
    }

    [Fact]
    public async Task RegionControlsDefaultHostAndNamespace()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler, options =>
        {
            options.Region = WowRegion.Eu;
            options.ApiBaseUrl = null;
        });

        await client.GameData.GetItemAsync(19019);

        var request = handler.ApiRequest();
        Assert.Equal("eu.api.blizzard.com", request.Uri.Host);
        Assert.Equal("static-eu", request.Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task SearchAddsQueryParameters()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item-search.json"));
        var client = TestClient.Create(handler);

        var response = await client.GameData.SearchItemsAsync("thunderfury", page: 2, pageSize: 50);

        var request = handler.ApiRequest();
        Assert.Equal("/data/wow/search/item", request.Uri.AbsolutePath);
        Assert.Equal("thunderfury", request.Uri.QueryValue("name.en_US"));
        Assert.Equal("id", request.Uri.QueryValue("orderby"));
        Assert.Equal("2", request.Uri.QueryValue("_page"));
        Assert.Equal("50", request.Uri.QueryValue("_pageSize"));
        Assert.Equal("static-us", request.Uri.QueryValue("namespace"));
        Assert.Null(request.Uri.QueryValue("q"));
        Assert.Null(request.Uri.QueryValue("page"));
        Assert.Null(request.Uri.QueryValue("page_size"));

        var result = Assert.Single(response.Results!);
        Assert.Equal(19019, result.Data!.Id);
        Assert.Equal("Thunderfury, Blessed Blade of the Windseeker", result.Data.Name!.EnglishUs);
    }

    [Fact]
    public async Task SearchUsesRequestLocaleInTheFieldName()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item-search.json"));
        var client = TestClient.Create(handler);

        await client.GameData.SearchItemsAsync(
            "Donnerzorn",
            requestOptions: new WowRequestOptions { Locale = WowLocale.GermanDe });

        var request = handler.ApiRequest();
        Assert.Equal("Donnerzorn", request.Uri.QueryValue("name.de_DE"));
        Assert.Null(request.Uri.QueryValue("name.en_US"));
        Assert.Equal("de_DE", request.Uri.QueryValue("locale"));
    }

    [Fact]
    public async Task SearchAllFollowsPagesUntilPageCount()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(
            """{"page":1,"pageSize":100,"maxPageSize":100,"pageCount":2,"resultCountCapped":false,"results":[{"key":{"href":"/1"},"data":{"id":1,"name":{"en_US":"One"}}}]}""");
        handler.EnqueueJson(
            """{"page":2,"pageSize":100,"maxPageSize":100,"pageCount":2,"resultCountCapped":false,"results":[{"key":{"href":"/2"},"data":{"id":2,"name":{"en_US":"Two"}}}]}""");
        var client = TestClient.Create(handler);

        var ids = new List<int>();
        await foreach (var result in client.GameData.SearchAllItemsAsync("item"))
        {
            ids.Add(result.Data!.Id);
        }

        Assert.Equal(new[] { 1, 2 }, ids);
        Assert.Equal(3, handler.RequestCount);
    }

    [Fact]
    public async Task SearchAllContinuesPastAnEmptyPageWhenPageCountRemains()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(
            """{"page":1,"pageSize":100,"maxPageSize":100,"pageCount":2,"resultCountCapped":false,"results":[]}""");
        handler.EnqueueJson(
            """{"page":2,"pageSize":100,"maxPageSize":100,"pageCount":2,"resultCountCapped":false,"results":[{"key":{"href":"/2"},"data":{"id":2,"name":{"en_US":"Two"}}}]}""");
        var client = TestClient.Create(handler);

        var ids = new List<int>();
        await foreach (var result in client.GameData.SearchAllItemsAsync("item"))
        {
            ids.Add(result.Data!.Id);
        }

        Assert.Equal(new[] { 2 }, ids);
        Assert.Equal(3, handler.RequestCount);
    }

    [Fact]
    public void SearchAllValidatesTheQueryBeforeEnumeration()
    {
        var handler = new FakeHttpMessageHandler();
        var client = TestClient.Create(handler);

        // The enumerable is returned eagerly; validation must not wait for first enumeration.
        Assert.ThrowsAny<ArgumentException>(() => client.GameData.SearchAllItemsAsync(" "));

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task SearchAllPropagatesCancellation()
    {
        // No responses queued: a pre-cancelled token must abort before any HTTP request.
        var handler = new FakeHttpMessageHandler();
        var client = TestClient.Create(handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var results = client.GameData.SearchAllItemsAsync(
            "item",
            cancellationToken: cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in results)
            {
            }
        });
    }

    [Fact]
    public async Task RawRequestReturnsJsonDocument()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("""{"id":19019,"name":"Thunderfury"}""");
        var client = TestClient.Create(handler);

        using JsonDocument document = await client.GetRawAsync(
            "/data/wow/item/19019",
            new WowRequestOptions { Namespace = "static-us" });

        Assert.Equal(19019, document.RootElement.GetProperty("id").GetInt32());
        Assert.Equal("static-us", handler.ApiRequest().Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task InvalidSearchPagingIsRejectedBeforeHttp()
    {
        var handler = new FakeHttpMessageHandler();
        var client = TestClient.Create(handler);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.GameData.SearchItemsAsync("item", page: 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.GameData.SearchItemsAsync("item", pageSize: 101));

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public void SearchResponseModelExposesPagingMetadata()
    {
        var response = new WowSearchResponse<ItemSearchResult>
        {
            Page = 1,
            PageSize = 100,
            MaxPageSize = 100,
            PageCount = 3,
            ResultCountCapped = true,
        };

        Assert.Equal(3, response.PageCount);
        Assert.Equal(100, response.MaxPageSize);
        Assert.True(response.ResultCountCapped);
    }
}
