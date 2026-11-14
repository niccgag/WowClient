using System.Text.Json;
using WowClient.Exceptions;
using WowClient.Tests.Helpers;

namespace WowClient.Tests;

public sealed class RawRequestTests
{
    [Theory]
    [InlineData("/data/wow/item/19019", "static-us")]
    [InlineData("/data/wow/realm/index", "dynamic-us")]
    [InlineData("/data/wow/connected-realm/1", "dynamic-us")]
    [InlineData("/profile/wow/character/tarren-mill/Fea", "profile-us")]
    [InlineData("/data/wow/guild/tarren-mill/example", "profile-us")]
    [InlineData("/data/wow/guild-crest/index", "static-us")]
    public async Task RawPathInfersNamespace(string path, string expectedNamespace)
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("""{"ok":true}""");
        var client = TestClient.Create(handler);

        using var document = await client.GetRawAsync(path);

        Assert.Equal(expectedNamespace, handler.ApiRequest().Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task RawRequestStillAllowsExplicitNamespaceOverride()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("""{"ok":true}""");
        var client = TestClient.Create(handler);

        using var document = await client.GetRawAsync(
            "/data/wow/item/19019",
            new WowRequestOptions { Namespace = "static-classic-us" });

        Assert.Equal("static-classic-us", handler.ApiRequest().Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task EmbeddedQueryStringIsPreserved()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("""{"ok":true}""");
        var client = TestClient.Create(handler);

        using var document = await client.GetRawAsync(
            "/data/wow/item/19019?foo=bar&namespace=static-classic-us");

        var request = handler.ApiRequest();
        Assert.Equal("bar", request.Uri.QueryValue("foo"));
        Assert.Equal("static-classic-us", request.Uri.QueryValue("namespace"));
        Assert.Equal("en_US", request.Uri.QueryValue("locale"));
    }

    [Fact]
    public async Task QueryParametersOverrideEmbeddedValues()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("""{"ok":true}""");
        var client = TestClient.Create(handler);

        using var document = await client.GetRawAsync(
            "/data/wow/item/19019?foo=bar",
            queryParameters: new Dictionary<string, string?> { ["foo"] = "baz" });

        Assert.Equal("baz", handler.ApiRequest().Uri.QueryValue("foo"));
    }

    [Fact]
    public async Task ExplicitRequestOptionsWinOverQueryParameters()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("""{"ok":true}""");
        var client = TestClient.Create(handler);

        using var document = await client.GetRawAsync(
            "/data/wow/item/19019",
            new WowRequestOptions { Namespace = "static-classic-us", Locale = WowLocale.GermanDe },
            new Dictionary<string, string?>
            {
                ["namespace"] = "static-eu",
                ["locale"] = "fr_FR",
            });

        var request = handler.ApiRequest();
        Assert.Equal("static-classic-us", request.Uri.QueryValue("namespace"));
        Assert.Equal("de_DE", request.Uri.QueryValue("locale"));
    }

    [Fact]
    public async Task QueryParametersProvideNamespaceWhenNoRequestOptionIsGiven()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("""{"ok":true}""");
        var client = TestClient.Create(handler);

        using var document = await client.GetRawAsync(
            "/data/wow/item/19019",
            queryParameters: new Dictionary<string, string?> { ["namespace"] = "static-classic-us" });

        Assert.Equal("static-classic-us", handler.ApiRequest().Uri.QueryValue("namespace"));
    }

    [Fact]
    public async Task MalformedRawJsonIsWrappedInApiException()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("{not json");
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GetRawAsync("/data/wow/item/19019"));

        Assert.IsAssignableFrom<JsonException>(exception.InnerException);
    }
}
