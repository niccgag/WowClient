using System.Net;
using WowClient.Auth;
using WowClient.Exceptions;
using WowClient.Tests.Helpers;

namespace WowClient.Tests;

public sealed class AuthTests
{
    [Fact]
    public async Task TokenRequestUsesBasicAuthAndClientCredentialsGrant()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler);

        var item = await client.GameData.GetItemAsync(19019);

        Assert.Equal(19019, item.Id);
        Assert.Equal(2, handler.RequestCount);

        var tokenRequest = handler.TokenRequest();
        Assert.Equal(HttpMethod.Post, tokenRequest.Method);
        Assert.Equal("https://oauth.test/token", tokenRequest.Uri.ToString());
        Assert.Equal(
            "test-client:test-secret",
            AuthHeader.DecodeBasicCredentials(tokenRequest.Authorization!));
        Assert.Equal("grant_type=client_credentials", tokenRequest.Body);

        Assert.Equal("Bearer test-token", handler.ApiRequest().Authorization);
    }

    [Fact]
    public async Task TokenIsCachedAcrossRequests()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item.json"));
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler);

        await client.GameData.GetItemAsync(1);
        await client.GameData.GetItemAsync(2);

        Assert.Equal(1, handler.TokenRequestCount);
        Assert.Equal(3, handler.RequestCount);
    }

    [Fact]
    public async Task TokenIsRefreshedAfterExpiry()
    {
        var timeProvider = new FakeTimeProvider();
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken(expiresIn: 120);
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler, timeProvider: timeProvider);

        await client.GameData.GetItemAsync(1);
        timeProvider.Advance(TimeSpan.FromSeconds(61));

        handler.EnqueueToken(expiresIn: 120);
        handler.EnqueueJson(Fixtures.Load("item.json"));
        await client.GameData.GetItemAsync(1);

        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task UnauthorizedResponseRefreshesTokenAndRetriesOnce()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(HttpStatusCode.Unauthorized);
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler);

        var item = await client.GameData.GetItemAsync(1);

        Assert.Equal(19019, item.Id);
        Assert.Equal(2, handler.TokenRequestCount);
        Assert.Equal(4, handler.RequestCount);
        Assert.Equal("Bearer test-token", handler.ApiRequest().Authorization);
    }

    [Fact]
    public async Task TokenFailureThrowsAuthenticationException()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueStatus(
            HttpStatusCode.Unauthorized,
            """{"error":"invalid_client","error_description":"Invalid client credentials"}""");
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowAuthenticationException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("invalid_client", exception.Error);
        Assert.Equal("Invalid client credentials", exception.ErrorDescription);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task MissingCredentialsThrowBeforeAnyHttpRequest()
    {
        var handler = new FakeHttpMessageHandler();
        var client = TestClient.Create(handler, options =>
        {
            options.ClientId = string.Empty;
            options.ClientSecret = string.Empty;
        });

        await Assert.ThrowsAsync<WowAuthenticationException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task ConcurrentRequestsShareASingleTokenRequest()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        for (var i = 0; i < 10; i++)
        {
            handler.EnqueueJson(Fixtures.Load("item.json"));
        }

        var client = TestClient.Create(handler);

        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => client.GameData.GetItemAsync(1)));

        Assert.Equal(1, handler.TokenRequestCount);
        Assert.Equal(11, handler.RequestCount);
    }
}
