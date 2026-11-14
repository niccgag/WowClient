using System.Text.Json;
using WowClient.Models;
using WowClient.Tests.Helpers;

namespace WowClient.Tests;

public sealed class DeserializationTests
{
    [Fact]
    public async Task ItemFixtureMapsCoreFieldsAndKeepsUnknownData()
    {
        var client = TestClient.ForFixture("item.json");

        var item = await client.GameData.GetItemAsync(19019);

        Assert.Equal(19019, item.Id);
        Assert.Equal("Thunderfury, Blessed Blade of the Windseeker", item.Name);
        Assert.Equal("LEGENDARY", item.Quality!.Type);
        Assert.Equal("Legendary", item.Quality.Name);
        Assert.Equal("Weapon", item.ItemClass!.Name);
        Assert.Equal(620187, item.SellPrice);
        Assert.True(item.IsEquippable);
        Assert.False(item.IsStackable);
        Assert.True(item.AdditionalProperties!.ContainsKey("preview_item"));
    }

    [Fact]
    public async Task ItemMediaFixtureMapsAssets()
    {
        var client = TestClient.ForFixture("item-media.json");

        var media = await client.GameData.GetItemMediaAsync(19019);

        Assert.Equal(19019, media.Id);
        var asset = Assert.Single(media.Assets!);
        Assert.Equal("icon", asset.Key);
        Assert.StartsWith("https://", asset.Value);
    }

    [Fact]
    public async Task RealmIndexFixtureMapsRealms()
    {
        var client = TestClient.ForFixture("realm-index.json");

        var index = await client.GameData.GetRealmIndexAsync();

        Assert.Equal(2, index.Realms!.Count);
        Assert.Equal("Tarren Mill", index.Realms[0].Name);
        Assert.Equal("tarren-mill", index.Realms[0].Slug);
        Assert.Equal(1096, index.Realms[0].Id);
    }

    [Fact]
    public async Task SpellFixtureMapsDescription()
    {
        var client = TestClient.ForFixture("spell.json");

        var spell = await client.GameData.GetSpellAsync(133);

        Assert.Equal(133, spell.Id);
        Assert.Equal("Fireball", spell.Name);
        Assert.Contains("Fire damage", spell.Description);
    }

    [Fact]
    public async Task MountFixtureMapsSourceAndFaction()
    {
        var client = TestClient.ForFixture("mount.json");

        var mount = await client.GameData.GetMountAsync(6);

        Assert.Equal("Brown Horse", mount.Name);
        Assert.Equal("Alliance", mount.Faction!.Name);
        Assert.Equal("VENDOR", mount.Source!.Type);
        Assert.NotNull(mount.CreatureDisplay!.Url);
    }

    [Fact]
    public async Task PetFixtureMapsAbilities()
    {
        var client = TestClient.ForFixture("pet.json");

        var pet = await client.GameData.GetPetAsync(39);

        Assert.Equal("Mechanical Squirrel", pet.Name);
        Assert.True(pet.IsBattlePet);
        Assert.True(pet.IsCapturable);
        var ability = Assert.Single(pet.Abilities!);
        Assert.Equal("Bite", ability.Name);
        Assert.Equal(1, ability.Slot);
    }

    [Fact]
    public async Task AchievementFixtureMapsCategoryAndPoints()
    {
        var client = TestClient.ForFixture("achievement.json");

        var achievement = await client.GameData.GetAchievementAsync(6);

        Assert.Equal("Level 10", achievement.Name);
        Assert.Equal("Characters", achievement.Category!.Name);
        Assert.Equal(10, achievement.Points);
        Assert.True(achievement.IsAccountWide);
    }

    [Fact]
    public async Task SearchFixtureMapsResults()
    {
        var client = TestClient.ForFixture("item-search.json");

        var response = await client.GameData.SearchItemsAsync("thunderfury");

        Assert.Equal(1, response.Page);
        Assert.Equal(100, response.PageSize);
        Assert.Equal(100, response.MaxPageSize);
        Assert.Equal(1, response.PageCount);
        Assert.False(response.ResultCountCapped);
        var result = Assert.Single(response.Results!);
        Assert.Equal(19019, result.Data!.Id);
        Assert.Equal("Thunderfury, Blessed Blade of the Windseeker", result.Data.Name!.EnglishUs);
        Assert.Equal("Legendär", result.Data.Quality!.Name!["de_DE"]);
        Assert.Equal("Sword", result.Data.ItemSubclass!.Name!.EnglishUs);
    }

    [Fact]
    public async Task SearchAcceptsAStringValuedLocalizedName()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(
            """{"page":1,"pageSize":100,"maxPageSize":100,"pageCount":1,"resultCountCapped":false,"results":[{"data":{"id":1,"name":"Thunderfury"}}]}""");
        var client = TestClient.Create(handler);

        var response = await client.GameData.SearchItemsAsync("thunderfury");

        var name = Assert.Single(response.Results!).Data!.Name!;
        Assert.Equal("Thunderfury", name.EnglishUs);
        Assert.Equal("Thunderfury", name.Get(WowLocale.EnglishUs));
        Assert.Equal("Thunderfury", name["en_US"]);
    }

    [Fact]
    public void LocalizedNameGetHandlesMissingAndNonStringValues()
    {
        using var english = JsonDocument.Parse("\"Thunderfury\"");
        using var number = JsonDocument.Parse("42");
        using var missing = JsonDocument.Parse("null");
        var name = new WowLocalizedName
        {
            Values = new Dictionary<string, JsonElement>(StringComparer.Ordinal)
            {
                ["en_US"] = english.RootElement.Clone(),
                ["de_DE"] = number.RootElement.Clone(),
                ["fr_FR"] = missing.RootElement.Clone(),
            },
        };

        Assert.Equal("Thunderfury", name.Get(WowLocale.EnglishUs));
        Assert.Equal("Thunderfury", name["en_US"]);
        Assert.Null(name.Get(WowLocale.GermanDe));
        Assert.Null(name.Get("es_MX"));
        Assert.ThrowsAny<ArgumentException>(() => name.Get(" "));
    }

    [Fact]
    public async Task SearchCapturesUnknownFieldsInAdditionalProperties()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.EnqueueJson(
            """{"page":1,"pageSize":100,"maxPageSize":100,"pageCount":1,"resultCountCapped":false,"future_field":{"a":1},"results":[{"data":{"id":1,"name":{"en_US":"One"},"extra_field":"x"}}]}""");
        var client = TestClient.Create(handler);

        var response = await client.GameData.SearchItemsAsync("one");

        Assert.True(response.AdditionalProperties!.ContainsKey("future_field"));
        Assert.Equal(
            JsonValueKind.Object,
            response.AdditionalProperties["future_field"].ValueKind);
        var data = Assert.Single(response.Results!).Data!;
        Assert.Equal("x", data.AdditionalProperties!["extra_field"].GetString());
    }

    [Fact]
    public async Task CharacterProfileFixtureMapsIdentityAndGuild()
    {
        var client = TestClient.ForFixture("character-profile.json");

        var profile = await client.Profile.GetCharacterProfileAsync("tarren-mill", "Fëa");

        Assert.Equal("Fëa", profile.Name);
        Assert.Equal("Priest", profile.CharacterClass!.Name);
        Assert.Equal("Shadow", profile.ActiveSpec!.Name);
        Assert.Equal("tarren-mill", profile.Realm!.Slug);
        Assert.Equal("Example Guild", profile.Guild!.Name);
        Assert.Equal(12345, profile.AchievementPoints);
        Assert.Equal(1000, profile.Achievements!.TotalQuantity);
        Assert.Equal(618, profile.EquippedItemLevel);
    }

    [Fact]
    public async Task CharacterEquipmentFixtureMapsItemsAndBonusLists()
    {
        var client = TestClient.ForFixture("character-equipment.json");

        var equipment = await client.Profile.GetCharacterEquipmentAsync("tarren-mill", "Fëa");

        Assert.Equal("Fëa", equipment.Character!.Name);
        var item = Assert.Single(equipment.EquippedItems!);
        Assert.Equal(19019, item.Item!.Id);
        Assert.Equal("Main Hand", item.Slot!.Name);
        Assert.Equal(new[] { 1234, 5678 }, item.BonusList);
        Assert.Equal(618, equipment.AverageItemLevel);
    }

    [Fact]
    public async Task CharacterProfessionsFixtureMapsTiersAndRecipes()
    {
        var client = TestClient.ForFixture("character-professions.json");

        var professions = await client.Profile.GetCharacterProfessionsAsync("tarren-mill", "Fëa");

        var primary = Assert.Single(professions.Primaries!);
        Assert.Equal("Alchemy", primary.ProfessionInfo!.Name);
        Assert.Equal("Classic", primary.Tiers![0].Tier!.Name);
        var recipe = Assert.Single(primary.Tiers[0].KnownRecipes!);
        Assert.Equal("Flask of the Titans", recipe.Name);
        Assert.Single(professions.Secondaries!);
    }

    [Fact]
    public async Task GuildFixtureMapsSummary()
    {
        var client = TestClient.ForFixture("guild.json");

        var guild = await client.Profile.GetGuildAsync("tarren-mill", "example-guild");

        Assert.Equal(42, guild.Id);
        Assert.Equal("Example Guild", guild.Name);
        Assert.Equal(25, guild.MemberCount);
        Assert.Equal(5000, guild.AchievementPoints);
        Assert.Equal("Tarren Mill", guild.Realm!.Name);
    }

    [Fact]
    public async Task GuildRosterFixtureMapsMembers()
    {
        var client = TestClient.ForFixture("guild-roster.json");

        var roster = await client.Profile.GetGuildRosterAsync("tarren-mill", "example-guild");

        Assert.Equal("Example Guild", roster.Guild!.Name);
        var member = Assert.Single(roster.Members!);
        Assert.Equal("Fëa", member.Character!.Name);
        Assert.Equal("Priest", member.Character.PlayableClass!.Name);
        Assert.Equal(0, member.Rank);
    }

    [Fact]
    public async Task CharacterStatusFixtureMapsValidity()
    {
        var client = TestClient.ForFixture("character-status.json");

        var status = await client.Profile.GetCharacterStatusAsync("tarren-mill", "Fëa");

        Assert.Equal(123, status.Id);
        Assert.True(status.IsValid);
    }

    [Fact]
    public async Task CharacterMountsFixtureMapsCollectionFlags()
    {
        var client = TestClient.ForFixture("character-mounts.json");

        var mounts = await client.Profile.GetCharacterMountsAsync("tarren-mill", "Fëa");

        var mount = Assert.Single(mounts.Mounts!);
        Assert.Equal(6, mount.Mount!.Id);
        Assert.True(mount.IsUseable);
        Assert.False(mount.IsFavorite);
    }

    [Fact]
    public async Task CharacterAchievementsFixtureMapsProgress()
    {
        var client = TestClient.ForFixture("character-achievements.json");

        var achievements = await client.Profile.GetCharacterAchievementsAsync("tarren-mill", "Fëa");

        Assert.Equal(1, achievements.TotalQuantity);
        Assert.Equal(10, achievements.TotalPoints);
        var achievement = Assert.Single(achievements.Achievements!);
        Assert.Equal("Level 10", achievement.Achievement!.Name);
        Assert.True(achievement.Criteria!.IsCompleted);
        Assert.Equal(10, achievement.Criteria.Amount);
    }

    [Fact]
    public async Task CharacterPvpSummaryFixtureMapsBracket()
    {
        var client = TestClient.ForFixture("character-pvp-summary.json");

        var summary = await client.Profile.GetCharacterPvpSummaryAsync("tarren-mill", "Fëa");

        Assert.Equal(5, summary.HonorLevel);
        var bracket = Assert.Single(summary.PvpBrackets!);
        Assert.Equal(1500, bracket.Rating);
        Assert.Equal("Challenger", bracket.Tier!.Name);
    }

    [Fact]
    public async Task MythicKeystoneProfileFixtureMapsRatingAndRuns()
    {
        var client = TestClient.ForFixture("mythic-keystone-profile.json");

        var profile = await client.Profile.GetCharacterMythicKeystoneProfileAsync("tarren-mill", "Fëa");

        Assert.Equal(1, profile.CurrentPeriod!.Id);
        Assert.Equal(2000.5, profile.CurrentMythicRating!.Rating);
        Assert.Equal(0.5, profile.CurrentMythicRating.Color!.G);
        var run = Assert.Single(profile.BestRuns!);
        Assert.Equal(20, run.KeystoneLevel);
        Assert.True(run.IsCompletedWithinTime);
        var member = Assert.Single(run.Members!);
        Assert.Equal("Shadow", member.Specialization!.Name);
        Assert.Equal(618, member.EquippedItemLevel);
    }

    [Fact]
    public async Task GuildActivityFixtureMapsAchievementEntry()
    {
        var client = TestClient.ForFixture("guild-activity.json");

        var activity = await client.Profile.GetGuildActivityAsync("tarren-mill", "example-guild");

        var entry = Assert.Single(activity.Activities!);
        Assert.Equal("Fëa", entry.CharacterAchievement!.Character!.Name);
        Assert.Equal("Level 10", entry.CharacterAchievement.Achievement!.Name);
        Assert.Equal(1700000000000, entry.Timestamp);
    }

    [Fact]
    public async Task GuildAchievementsFixtureMapsTotals()
    {
        var client = TestClient.ForFixture("guild-achievements.json");

        var achievements = await client.Profile.GetGuildAchievementsAsync("tarren-mill", "example-guild");

        Assert.Equal(1, achievements.TotalQuantity);
        Assert.Equal(10, achievements.TotalPoints);
        Assert.Single(achievements.Achievements!);
    }

    [Fact]
    public async Task ItemClassFixturesMapClassesAndSubclasses()
    {
        var index = await TestClient.ForFixture("item-class-index.json")
            .GameData.GetItemClassIndexAsync();
        var itemClass = await TestClient.ForFixture("item-class.json")
            .GameData.GetItemClassAsync(2);
        var subclass = await TestClient.ForFixture("item-subclass.json")
            .GameData.GetItemSubclassAsync(2, 7);

        Assert.Equal("Weapon", Assert.Single(index.Classes!).Name);
        Assert.Equal("Sword", Assert.Single(itemClass.ItemSubclasses!).Name);
        Assert.Equal("Sword", subclass.DisplayName);
        Assert.Equal(2, subclass.ClassId);
    }

    [Fact]
    public async Task IndexFixturesMapMountsPetsAndCategories()
    {
        var mounts = await TestClient.ForFixture("mount-index.json").GameData.GetMountIndexAsync();
        var pets = await TestClient.ForFixture("pet-index.json").GameData.GetPetIndexAsync();
        var categories = await TestClient.ForFixture("achievement-category-index.json")
            .GameData.GetAchievementCategoryIndexAsync();

        Assert.Equal("Brown Horse", Assert.Single(mounts.Mounts!).Name);
        Assert.Equal("Mechanical Squirrel", Assert.Single(pets.Pets!).Name);
        Assert.Equal("Characters", Assert.Single(categories.Categories!).Name);
    }

    [Fact]
    public async Task MediaFixturesMapSpellAndAchievementAssets()
    {
        var spellMedia = await TestClient.ForFixture("spell-media.json")
            .GameData.GetSpellMediaAsync(133);
        var achievementMedia = await TestClient.ForFixture("achievement-media.json")
            .GameData.GetAchievementMediaAsync(6);

        Assert.Equal("icon", Assert.Single(spellMedia.Assets!).Key);
        Assert.StartsWith("https://", Assert.Single(achievementMedia.Assets!).Value);
    }

    [Fact]
    public async Task ConnectedRealmFixtureMapsStatusAndRealms()
    {
        var client = TestClient.ForFixture("connected-realm.json");

        var realm = await client.GameData.GetConnectedRealmAsync(1);

        Assert.Equal("Up", realm.Status!.Name);
        Assert.Equal("High", realm.Population!.Name);
        Assert.Equal("tarren-mill", Assert.Single(realm.Realms!).Slug);
    }
}
