namespace WowClient;

/// <summary>
/// Builds the namespace identifiers used by the WoW APIs.
/// </summary>
public static class WowNamespaces
{
    /// <summary>Static game data namespace, such as <c>static-eu</c>.</summary>
    public static string Static(WowRegion region) => $"static-{region.ToSlug()}";

    /// <summary>Dynamic game data namespace, such as <c>dynamic-us</c>.</summary>
    public static string Dynamic(WowRegion region) => $"dynamic-{region.ToSlug()}";

    /// <summary>Profile namespace, such as <c>profile-kr</c>.</summary>
    public static string Profile(WowRegion region) => $"profile-{region.ToSlug()}";

    /// <summary>
    /// Infers the namespace required by a raw API path. The caller can still override it with
    /// <see cref="WowRequestOptions.Namespace"/>.
    /// </summary>
    public static string ForRawPath(string relativePath, WowRegion region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var path = relativePath;
        var queryIndex = path.IndexOf('?');
        if (queryIndex >= 0)
        {
            path = path[..queryIndex];
        }

        var fragmentIndex = path.IndexOf('#');
        if (fragmentIndex >= 0)
        {
            path = path[..fragmentIndex];
        }

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        // The trailing slash keeps /data/wow/guild-crest/... (a static-namespace family) out of the
        // profile match while still covering /data/wow/guild/{realm}/{guild}/....
        if (path.Equals("/profile", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/profile/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/data/wow/guild", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/data/wow/guild/", StringComparison.OrdinalIgnoreCase))
        {
            return Profile(region);
        }

        if (path.StartsWith("/data/wow/realm", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/data/wow/connected-realm", StringComparison.OrdinalIgnoreCase))
        {
            return Dynamic(region);
        }

        return Static(region);
    }
}
