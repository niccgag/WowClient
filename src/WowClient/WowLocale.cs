namespace WowClient;

/// <summary>
/// Locale identifiers accepted by the Blizzard APIs. Availability depends on the region.
/// </summary>
public static class WowLocale
{
    public const string EnglishUs = "en_US";
    public const string EnglishGb = "en_GB";
    public const string SpanishMx = "es_MX";
    public const string SpanishEs = "es_ES";
    public const string PortugueseBr = "pt_BR";
    public const string FrenchFr = "fr_FR";
    public const string GermanDe = "de_DE";
    public const string ItalianIt = "it_IT";
    public const string RussianRu = "ru_RU";
    public const string KoreanKr = "ko_KR";
    public const string ChineseTw = "zh_TW";

    /// <summary>
    /// Gets every locale known to the client.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(new[]
    {
        EnglishUs,
        EnglishGb,
        SpanishMx,
        SpanishEs,
        PortugueseBr,
        FrenchFr,
        GermanDe,
        ItalianIt,
        RussianRu,
        KoreanKr,
        ChineseTw,
    });
}
