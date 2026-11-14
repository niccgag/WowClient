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
    public async Task RateLimitedRequestIsSurfacedWhenDelayWouldOutlastTheTimeout()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(
            (HttpStatusCode)429,
            """{"code":429,"type":"BLIZZARD","detail":"Rate limited"}""",
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(2)));
        // No second API response: the delay must not be attempted, because it would be cancelled by
        // the 100 ms operation timeout and surface as a misleading timeout instead of the 429.
        var client = TestClient.Create(handler, options => options.Timeout = TimeSpan.FromMilliseconds(100));

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(2), exception.RetryAfter);
        Assert.Single(handler.Requests, request => request.Uri.Host == "api.test");
    }

    [Fact]
    public async Task RetryAfterIsExposedOnNonRateLimitErrors()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(
            HttpStatusCode.ServiceUnavailable,
            """{"code":503,"type":"BLIZZARD","detail":"Temporarily unavailable"}""",
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30)));
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(30), exception.RetryAfter);
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

    [Fact]
    public async Task DateBasedRetryAfterUsesTheInjectedTimeProvider()
    {
        var timeProvider = new FakeTimeProvider
        {
            UtcNow = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(
            (HttpStatusCode)429,
            """{"code":429,"type":"BLIZZARD","detail":"Rate limited"}""",
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(
                new DateTimeOffset(2020, 1, 1, 0, 0, 10, TimeSpan.Zero)));
        // Deliberately do not enqueue a second API response. If the client compares the date with the
        // real clock it treats the header as already expired and retries, which fails the test.
        var client = TestClient.Create(handler, timeProvider: timeProvider);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(10), exception.RetryAfter);
        Assert.Single(handler.Requests, request => request.Uri.Host == "api.test");
    }

    [Fact]
    public async Task EmptySuccessBodyThrowsProtocolException()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(string.Empty);
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowProtocolException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal("api.test", exception.RequestUri!.Host);
        Assert.Equal("/data/wow/item/1", exception.RequestUri.AbsolutePath);
    }

    [Fact]
    public async Task NoContentResponseThrowsProtocolException()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueStatus(HttpStatusCode.NoContent);
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowProtocolException>(
            () => client.GameData.GetItemAsync(1));

        Assert.Equal("api.test", exception.RequestUri!.Host);
    }

    [Fact]
    public async Task MalformedSuccessBodyThrowsApiExceptionWithJsonInnerException()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson("{not json");
        var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<WowApiException>(
            () => client.GameData.GetItemAsync(1));

        Assert.IsAssignableFrom<System.Text.Json.JsonException>(exception.InnerException);
    }
}
