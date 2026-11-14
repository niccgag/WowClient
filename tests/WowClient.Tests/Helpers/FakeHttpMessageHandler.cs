using System.Net;
using System.Text;

namespace WowClient.Tests.Helpers;

internal sealed record CapturedRequest(
    HttpMethod Method,
    Uri Uri,
    string? Authorization,
    string? Body,
    IReadOnlyDictionary<string, string> Headers);

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();
    private readonly object _gate = new();

    public List<CapturedRequest> Requests { get; } = new();

    public int RequestCount => Requests.Count;

    public int TokenRequestCount => Requests.Count(request => request.Uri.AbsolutePath == "/token");

    public bool IsDisposed { get; private set; }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }

    public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        _responses.Enqueue(responder);

    public void EnqueueJson(
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        Action<HttpResponseMessage>? configure = null) =>
        Enqueue(_ =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
            configure?.Invoke(response);
            return response;
        });

    public void EnqueueStatus(
        HttpStatusCode statusCode,
        string? json = null,
        Action<HttpResponseMessage>? configure = null) =>
        Enqueue(_ =>
        {
            var response = new HttpResponseMessage(statusCode);
            if (json is not null)
            {
                response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            configure?.Invoke(response);
            return response;
        });

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var captured = new CapturedRequest(
            request.Method,
            request.RequestUri!,
            request.Headers.Authorization?.ToString(),
            body,
            request.Headers.ToDictionary(
                header => header.Key,
                header => string.Join(",", header.Value),
                StringComparer.OrdinalIgnoreCase));

        Func<HttpRequestMessage, HttpResponseMessage> responder;
        lock (_gate)
        {
            Requests.Add(captured);

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No response queued for {request.Method} {request.RequestUri}.");
            }

            responder = _responses.Dequeue();
        }

        return responder(request);
    }
}
