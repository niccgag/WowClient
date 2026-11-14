using System.Text.Json;
using WowClient.Models;
using WowClient.Models.GameData;
using WowClient.WebApi.Contracts;

namespace WowClient.WebApi.Tests;

public sealed class ResponseContractTests
{
    [Fact]
    public void ItemSearchResponsePrefersEnglishUs()
    {
        var item = new ItemSearchResult
        {
            Id = 19019,
            Name = Localized(("en_US", "Thunderfury"), ("de_DE", "Donnerzorn")),
        };

        Assert.Equal("Thunderfury", ItemResponse.From(item).Name);
    }

    [Fact]
    public void ItemSearchResponseFallsBackToTheFirstAvailableLocale()
    {
        var item = new ItemSearchResult
        {
            Id = 19019,
            Name = Localized(("de_DE", "Donnerzorn")),
            Quality = new WowSearchName { Name = Localized(("fr_FR", "Légendaire")) },
            ItemClass = new WowSearchName { Name = Localized(("de_DE", "Waffe")) },
        };

        var response = ItemResponse.From(item);

        Assert.Equal("Donnerzorn", response.Name);
        Assert.Equal("Légendaire", response.Quality);
        Assert.Equal("Waffe", response.ItemClass);
    }

    [Fact]
    public void ItemSearchResponseIgnoresNonStringLocalizedValues()
    {
        var item = new ItemSearchResult
        {
            Id = 1,
            Name = Localized(("de_DE", 42)),
        };

        Assert.Null(ItemResponse.From(item).Name);
    }

    private static WowLocalizedName Localized(params (string Locale, object Value)[] entries)
    {
        var values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var (locale, value) in entries)
        {
            values[locale] = JsonSerializer.SerializeToElement(value);
        }

        return new WowLocalizedName { Values = values };
    }
}
