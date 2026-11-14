using WowClient.Tests.Helpers;

namespace WowClient.Tests;

public sealed class ClientLifetimeTests
{
    [Fact]
    public void DisposeDoesNotDisposeCallerProvidedClients()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var client = global::WowClient.WowClient.Create(TestClient.Options(), httpClient, httpClient);

        client.Dispose();
        client.Dispose();

        Assert.False(handler.IsDisposed);

        httpClient.Dispose();
        Assert.True(handler.IsDisposed);
    }

    [Fact]
    public void DisposeIsSafeForInternallyCreatedClients()
    {
        var client = global::WowClient.WowClient.Create(TestClient.Options());

        client.Dispose();
        client.Dispose();
    }

    [Fact]
    public void CreateRejectsNonPositiveTimeout()
    {
        var options = TestClient.Options(configure => configure.Timeout = TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => global::WowClient.WowClient.Create(options));

        Assert.Contains("Timeout", exception.Message);
    }

    [Fact]
    public void CreateRejectsRelativeBaseUrl()
    {
        var options = TestClient.Options(configure =>
            configure.ApiBaseUrl = new Uri("relative", UriKind.Relative));

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("ApiBaseUrl", exception.Message);
    }

    [Fact]
    public void CreateRejectsNegativeRetrySettings()
    {
        var options = TestClient.Options(configure => configure.MaxRateLimitRetries = -1);

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("MaxRateLimitRetries", exception.Message);
    }

    [Theory]
    [InlineData("https://api.test/base?x=1", "query string")]
    [InlineData("https://api.test/base#frag", "fragment")]
    [InlineData("https://user:pass@api.test", "user info")]
    [InlineData("http://api.test", "loopback")]
    public void CreateRejectsInvalidApiBaseUrl(string baseUrl, string expectedReason)
    {
        var options = TestClient.Options(configure => configure.ApiBaseUrl = new Uri(baseUrl));

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("ApiBaseUrl", exception.Message);
        Assert.Contains(expectedReason, exception.Message);
    }

    [Theory]
    [InlineData("https://oauth.test?x=1", "query string")]
    [InlineData("https://oauth.test#frag", "fragment")]
    [InlineData("https://user:pass@oauth.test", "user info")]
    [InlineData("http://oauth.test", "loopback")]
    [InlineData("ftp://oauth.test", "https")]
    public void CreateRejectsInvalidOAuthBaseUrl(string baseUrl, string expectedReason)
    {
        var options = TestClient.Options(configure => configure.OAuthBaseUrl = new Uri(baseUrl));

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("OAuthBaseUrl", exception.Message);
        Assert.Contains(expectedReason, exception.Message);
    }

    [Fact]
    public void CreateRejectsRelativeOAuthBaseUrl()
    {
        var options = TestClient.Options(configure =>
            configure.OAuthBaseUrl = new Uri("relative", UriKind.Relative));

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("OAuthBaseUrl", exception.Message);
        Assert.Contains("absolute", exception.Message);
    }

    [Fact]
    public void CreateAcceptsLoopbackHttpBaseUrls()
    {
        var options = TestClient.Options(configure =>
        {
            configure.ApiBaseUrl = new Uri("http://localhost:5000", UriKind.Absolute);
            configure.OAuthBaseUrl = new Uri("http://127.0.0.1:9000", UriKind.Absolute);
        });

        using var client = global::WowClient.WowClient.Create(options);

        Assert.NotNull(client);
    }

    [Fact]
    public void CreateRejectsOversizedTimeout()
    {
        var options = TestClient.Options(configure => configure.Timeout = TimeSpan.MaxValue);

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("Timeout", exception.Message);
    }

    [Fact]
    public void CreateRejectsOversizedMaxRateLimitDelay()
    {
        var options = TestClient.Options(configure => configure.MaxRateLimitDelay = TimeSpan.MaxValue);

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("MaxRateLimitDelay", exception.Message);
    }

    [Fact]
    public void CreateRejectsNegativeMaxRateLimitDelay()
    {
        var options = TestClient.Options(configure => configure.MaxRateLimitDelay = TimeSpan.FromSeconds(-1));

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("MaxRateLimitDelay", exception.Message);
    }

    [Fact]
    public void CreateRejectsEmptyLocale()
    {
        var options = TestClient.Options(configure => configure.Locale = " ");

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("Locale", exception.Message);
    }

    [Fact]
    public void CreateRejectsEmptyUserAgent()
    {
        var options = TestClient.Options(configure => configure.UserAgent = string.Empty);

        var exception = Assert.Throws<ArgumentException>(() => global::WowClient.WowClient.Create(options));

        Assert.Contains("UserAgent", exception.Message);
    }

    [Fact]
    public void DisposeLeavesTheCallerProvidedClientWhenOnlyOneIsSupplied()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var client = global::WowClient.WowClient.Create(
            TestClient.Options(),
            apiClient: httpClient,
            authClient: null);

        client.Dispose();

        Assert.False(handler.IsDisposed);

        httpClient.Dispose();
        Assert.True(handler.IsDisposed);
    }

    [Fact]
    public void ConcurrentDisposeIsSafe()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var client = global::WowClient.WowClient.Create(TestClient.Options(), httpClient, httpClient);

        Parallel.For(0, 16, _ => client.Dispose());

        Assert.False(handler.IsDisposed);
        httpClient.Dispose();
    }

    [Fact]
    public void LocaleListCannotBeCastToMutableArray()
    {
        Assert.False(WowLocale.All is string[]);
        Assert.Equal(WowLocale.EnglishUs, WowLocale.All[0]);
    }
}
