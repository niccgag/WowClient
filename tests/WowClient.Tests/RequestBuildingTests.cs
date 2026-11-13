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
        Assert.Equal("thunderfury", request.Uri.QueryValue("q"));
        Assert.Equal("2", request.Uri.QueryValue("page"));
        Assert.Equal("50", request.Uri.QueryValue("page_size"));
        Assert.Equal("static-us", request.Uri.QueryValue("namespace"));

        var result = Assert.Single(response.Results!);
        Assert.Equal(19019, result.Data!.Id);
    }

    [Fact]
    public async Task SearchAllFollowsPagesUntilMaxPage()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(
            """{"page":1,"page_size":1000,"max_page":2,"total_count":2,"results":[{"key":{"href":"/1"},"data":{"id":1,"name":"One"}}]}""");
        handler.EnqueueJson(
            """{"page":2,"page_size":1000,"max_page":2,"total_count":2,"results":[{"key":{"href":"/2"},"data":{"id":2,"name":"Two"}}]}""");
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
            () => client.GameData.SearchItemsAsync("item", pageSize: 1001));

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public void SearchResponseModelExposesPagingMetadata()
    {
        var response = new WowSearchResponse<Item>
        {
            Page = 1,
            PageSize = 100,
            MaxPage = 3,
            TotalCount = 250,
        };

        Assert.Equal(3, response.MaxPage);
        Assert.Equal(250, response.TotalCount);
    }
}
