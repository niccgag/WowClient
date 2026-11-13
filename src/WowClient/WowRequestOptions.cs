namespace WowClient;

/// <summary>
/// Per-request overrides for the namespace and locale query parameters.
/// </summary>
public sealed record WowRequestOptions
{
    /// <summary>
    /// Overrides the default namespace for the endpoint, for example <c>static-classic-us</c>.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Overrides the locale configured on <see cref="WowClientOptions.Locale"/>.
    /// </summary>
    public string? Locale { get; init; }
}
