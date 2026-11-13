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
        Assert.Equal(1, response.MaxPage);
        Assert.Equal(1, response.TotalCount);
        var result = Assert.Single(response.Results!);
        Assert.Equal(19019, result.Data!.Id);
        Assert.Equal("Thunderfury, Blessed Blade of the Windseeker", result.Data.Name);
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
}
