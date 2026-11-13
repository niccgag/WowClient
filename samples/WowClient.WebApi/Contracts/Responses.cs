using WowClient.Models.GameData;
using WowClient.Models.Profile;

namespace WowClient.WebApi.Contracts;

public sealed record ServiceInfoResponse(string Service, string OpenApi);

public sealed record HealthResponse(string Status);

public sealed record RealmResponse(int Id, string? Name, string? Slug, string? Timezone, string? Category)
{
    public static RealmResponse From(RealmReference realm) =>
        new(realm.Id, realm.Name, realm.Slug, null, null);

    public static RealmResponse From(Realm realm) =>
        new(realm.Id, realm.Name, realm.Slug, realm.Timezone, realm.Category);
}

public sealed record ItemResponse(
    int Id,
    string? Name,
    string? Quality,
    string? ItemClass,
    string? ItemSubclass,
    string? InventoryType,
    int Level,
    int RequiredLevel,
    bool IsEquippable)
{
    public static ItemResponse From(Item item) => new(
        item.Id,
        item.Name,
        item.Quality?.Name,
        item.ItemClass?.Name,
        item.ItemSubclass?.Name,
        item.InventoryType?.Name,
        item.Level,
        item.RequiredLevel,
        item.IsEquippable);
}

public sealed record ItemSearchResponse(
    int Page,
    int PageSize,
    int MaxPage,
    int TotalCount,
    IReadOnlyList<ItemResponse> Results)
{
    public static ItemSearchResponse From(WowSearchResponse<Item> response)
    {
        var results = response.Results?
            .Where(result => result.Data is not null)
            .Select(result => ItemResponse.From(result.Data!))
            .ToArray() ?? [];

        return new ItemSearchResponse(
            response.Page,
            response.PageSize,
            response.MaxPage,
            response.TotalCount,
            results);
    }
}

public sealed record SpellResponse(int Id, string? Name, string? Description)
{
    public static SpellResponse From(Spell spell) => new(spell.Id, spell.Name, spell.Description);
}

public sealed record MountResponse(int Id, string? Name, string? Faction, string? Source)
{
    public static MountResponse From(MountReference mount) =>
        new(mount.Id, mount.Name, null, null);
}

public sealed record CharacterResponse(
    int Id,
    string? Name,
    string? Realm,
    int Level,
    string? Race,
    string? CharacterClass,
    string? Faction,
    string? Guild,
    int AverageItemLevel,
    int EquippedItemLevel,
    int AchievementPoints)
{
    public static CharacterResponse From(CharacterProfile profile) => new(
        profile.Id,
        profile.Name,
        profile.Realm?.Name,
        profile.Level,
        profile.Race?.Name,
        profile.CharacterClass?.Name,
        profile.Faction?.Name,
        profile.Guild?.Name,
        profile.AverageItemLevel,
        profile.EquippedItemLevel,
        profile.AchievementPoints);
}

public sealed record EquippedItemResponse(int Id, string? Name, string? Slot, string? Quality);

public sealed record CharacterEquipmentResponse(
    string? Character,
    string? Realm,
    int AverageItemLevel,
    IReadOnlyList<EquippedItemResponse> Items)
{
    public static CharacterEquipmentResponse From(CharacterEquipment equipment)
    {
        var items = equipment.EquippedItems?
            .Select(item => new EquippedItemResponse(
                item.Item?.Id ?? 0,
                item.Item?.Name,
                item.Slot?.Name,
                item.Quality?.Name))
            .ToArray() ?? [];

        return new CharacterEquipmentResponse(
            equipment.Character?.Name,
            equipment.Character?.Realm?.Name,
            equipment.AverageItemLevel,
            items);
    }
}

public sealed record GuildMemberResponse(
    string? Name,
    int Level,
    string? CharacterClass,
    string? Race,
    int Rank);

public sealed record GuildRosterResponse(
    string? Guild,
    string? Realm,
    IReadOnlyList<GuildMemberResponse> Members)
{
    public static GuildRosterResponse From(GuildRoster roster)
    {
        var members = roster.Members?
            .Select(member => new GuildMemberResponse(
                member.Character?.Name,
                member.Character?.Level ?? 0,
                member.Character?.PlayableClass?.Name,
                member.Character?.PlayableRace?.Name,
                member.Rank))
            .ToArray() ?? [];

        return new GuildRosterResponse(roster.Guild?.Name, roster.Guild?.Realm?.Name, members);
    }
}
