using System.Net;
using System.Net.Http.Headers;
using WowClient.Exceptions;
using WowClient.Tests.Helpers;

namespace WowClient.Tests;

public sealed class ErrorHandlingTests
{
    [Fact]
    public async Task NotFoundIncludesBlizzardErrorDetails()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(
            HttpStatusCode.NotFound,
            """{"code":404,"type":"BLIZZARD","detail":"The requested resource was not found."}""");
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(999999));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal(404, exception.ErrorCode);
        Assert.Equal("BLIZZARD", exception.ErrorType);
        Assert.Equal("The requested resource was not found.", exception.ErrorDetail);
        Assert.Equal("api.test", exception.RequestUri!.Host);
        Assert.Contains("status 404", exception.Message);
        Assert.Contains("code: 404", exception.Message);
    }

    [Fact]
    public async Task RateLimitedRequestIsRetriedWhenRetryAfterIsWithinLimit()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(
            (HttpStatusCode)429,
            """{"code":429,"type":"BLIZZARD","detail":"Rate limited"}""",
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero));
        handler.EnqueueJson(Fixtures.Load("item.json"));
        var client = TestClient.Create(handler);

        var item = await client.GameData.GetItemAsync(19019);

        Assert.Equal(19019, item.Id);
        Assert.Equal(2, handler.Requests.Count(request => request.Uri.Host == "api.test"));
    }

    [Fact]
    public async Task RateLimitedRequestWithoutRetryAfterThrows()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(
            (HttpStatusCode)429,
            """{"code":429,"type":"BLIZZARD","detail":"Rate limited"}""");
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
        Assert.Single(handler.Requests, request => request.Uri.Host == "api.test");
    }

    [Fact]
    public async Task RateLimitedRequestIsNotRetriedWhenDelayExceedsLimit()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(
            (HttpStatusCode)429,
            """{"code":429,"type":"BLIZZARD","detail":"Rate limited"}""",
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)));
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
        Assert.Single(handler.Requests, request => request.Uri.Host == "api.test");
    }

    [Fact]
    public async Task SecondUnauthorizedResponseIsSurfaced()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(HttpStatusCode.Unauthorized);
        handler.EnqueueToken();
        handler.EnqueueStatus(HttpStatusCode.Unauthorized, """{"code":401,"type":"BLIZZARD","detail":"Unauthorized"}""");
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal(2, handler.TokenRequestCount);
        Assert.Equal(4, handler.RequestCount);
    }

    [Fact]
    public async Task ServerErrorWithNonJsonBodyKeepsRawResponse()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(HttpStatusCode.BadGateway, "upstream unavailable");
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Null(exception.ErrorCode);
        Assert.Equal("upstream unavailable", exception.ResponseBody);
    }
}
