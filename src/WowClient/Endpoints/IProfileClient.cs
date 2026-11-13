using WowClient.Models.Profile;

namespace WowClient.Endpoints;

/// <summary>
/// Access to the WoW Profile API (characters and guilds).
/// </summary>
public interface IProfileClient
{
    /// <summary>Gets a character profile summary.</summary>
    Task<CharacterProfile> GetCharacterProfileAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets whether a character profile is currently available.</summary>
    Task<CharacterStatus> GetCharacterStatusAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a character's equipped items.</summary>
    Task<CharacterEquipment> GetCharacterEquipmentAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a character's mount collection.</summary>
    Task<CharacterMounts> GetCharacterMountsAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a character's achievements.</summary>
    Task<CharacterAchievements> GetCharacterAchievementsAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a character's professions.</summary>
    Task<CharacterProfessions> GetCharacterProfessionsAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a character's PvP summary.</summary>
    Task<CharacterPvpSummary> GetCharacterPvpSummaryAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a character's mythic keystone profile.</summary>
    Task<MythicKeystoneProfile> GetCharacterMythicKeystoneProfileAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a guild summary.</summary>
    Task<Guild> GetGuildAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a guild roster.</summary>
    Task<GuildRoster> GetGuildRosterAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets recent guild activity.</summary>
    Task<GuildActivity> GetGuildActivityAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a guild's achievements.</summary>
    Task<GuildAchievements> GetGuildAchievementsAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);
}
