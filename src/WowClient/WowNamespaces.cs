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
}
