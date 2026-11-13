namespace WowClient.Http;

/// <summary>
/// Minimal <see cref="IHttpClientFactory"/> that always returns the same client. Used by
/// <see cref="WowClient.Create"/> so callers can supply their own <see cref="HttpClient"/>.
/// </summary>
internal sealed class SingleHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public SingleHttpClientFactory(HttpClient client) => _client = client;

    public HttpClient CreateClient(string name) => _client;
}
