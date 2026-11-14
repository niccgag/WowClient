using System.Text;

namespace WowClient.Tests.Helpers;

internal static class TestClient
{
    public const string TokenJson =
        """{"access_token":"test-token","token_type":"bearer","expires_in":86399,"scope":""}""";

    public static WowClient Create(
        FakeHttpMessageHandler handler,
        Action<WowClientOptions>? configure = null,
        TimeProvider? timeProvider = null)
    {
        var options = Options(configure);
        var httpClient = new HttpClient(handler);
        return global::WowClient.WowClient.Create(options, httpClient, httpClient, timeProvider);
    }

    public static WowClientOptions Options(Action<WowClientOptions>? configure = null)
    {
        var options = new WowClientOptions
        {
            ClientId = "test-client",
            ClientSecret = "test-secret",
            Region = WowRegion.Us,
            Locale = WowLocale.EnglishUs,
            ApiBaseUrl = new Uri("https://api.test", UriKind.Absolute),
            OAuthBaseUrl = new Uri("https://oauth.test", UriKind.Absolute),
        };

        configure?.Invoke(options);
        return options;
    }

    public static void EnqueueToken(this FakeHttpMessageHandler handler, int expiresIn = 86399) =>
        handler.EnqueueJson(
            $$"""{"access_token":"test-token","token_type":"bearer","expires_in":{{expiresIn}}}""");

    public static global::WowClient.WowClient ForFixture(
        string fixtureName,
        Action<WowClientOptions>? configure = null,
        TimeProvider? timeProvider = null)
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(Fixtures.Load(fixtureName));
        return Create(handler, configure, timeProvider);
    }

    public static CapturedRequest ApiRequest(this FakeHttpMessageHandler handler) =>
        handler.Requests.Last(request => request.Uri.AbsolutePath != "/token");

    public static CapturedRequest TokenRequest(this FakeHttpMessageHandler handler) =>
        handler.Requests.First(request => request.Uri.AbsolutePath == "/token");

    public static string? QueryValue(this Uri uri, string key) =>
        uri.QueryParameters().TryGetValue(key, out var value) ? value : null;

    public static IReadOnlyDictionary<string, string> QueryParameters(this Uri uri) =>
        uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                part => Uri.UnescapeDataString(part[0]),
                part => part.Length > 1 ? Uri.UnescapeDataString(part[1]) : string.Empty,
                StringComparer.Ordinal);
}

internal sealed class FakeTimeProvider : TimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => UtcNow;

    public void Advance(TimeSpan delta) => UtcNow += delta;
}

internal static class Fixtures
{
    public static string Load(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
}

internal static class AuthHeader
{
    public static string DecodeBasicCredentials(string authorization)
    {
        const string prefix = "Basic ";
        if (!authorization.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected a Basic authorization header, got '{authorization}'.");
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(authorization[prefix.Length..]));
    }
}
