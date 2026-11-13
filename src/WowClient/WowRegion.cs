namespace WowClient;

/// <summary>
/// The Blizzard API region used to resolve hosts and namespaces.
/// </summary>
public enum WowRegion
{
    /// <summary>Americas, Oceania, and Southeast Asia.</summary>
    Us,

    /// <summary>Europe.</summary>
    Eu,

    /// <summary>Korea.</summary>
    Kr,

    /// <summary>Taiwan.</summary>
    Tw,

    /// <summary>China.</summary>
    Cn,
}

/// <summary>
/// Helpers for converting <see cref="WowRegion"/> values into API host and namespace fragments.
/// </summary>
public static class WowRegionExtensions
{
    /// <summary>
    /// Gets the lowercase region slug used by Blizzard (for example, <c>us</c> or <c>eu</c>).
    /// </summary>
    public static string ToSlug(this WowRegion region) => region switch
    {
        WowRegion.Us => "us",
        WowRegion.Eu => "eu",
        WowRegion.Kr => "kr",
        WowRegion.Tw => "tw",
        WowRegion.Cn => "cn",
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown region."),
    };

    /// <summary>
    /// Gets the regional API host, such as <c>eu.api.blizzard.com</c>.
    /// </summary>
    public static string ToApiHost(this WowRegion region) => $"{region.ToSlug()}.api.blizzard.com";
}
