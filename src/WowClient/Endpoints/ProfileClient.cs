using WowClient.Http;
using WowClient.Models.Profile;

namespace WowClient.Endpoints;

internal sealed class ProfileClient : IProfileClient
{
    private readonly WowApiRequestExecutor _executor;
    private readonly WowClientOptions _options;

    public ProfileClient(WowApiRequestExecutor executor, WowClientOptions options)
    {
        _executor = executor;
        _options = options;
    }

    private string ProfileNamespace => WowNamespaces.Profile(_options.Region);

    public Task<CharacterProfile> GetCharacterProfileAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<CharacterProfile>(
            realmSlug,
            characterName,
            string.Empty,
            requestOptions,
            cancellationToken);

    public Task<CharacterStatus> GetCharacterStatusAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<CharacterStatus>(
            realmSlug,
            characterName,
            "/status",
            requestOptions,
            cancellationToken);

    public Task<CharacterEquipment> GetCharacterEquipmentAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<CharacterEquipment>(
            realmSlug,
            characterName,
            "/equipment",
            requestOptions,
            cancellationToken);

    public Task<CharacterMounts> GetCharacterMountsAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<CharacterMounts>(
            realmSlug,
            characterName,
            "/collections/mounts",
            requestOptions,
            cancellationToken);

    public Task<CharacterAchievements> GetCharacterAchievementsAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<CharacterAchievements>(
            realmSlug,
            characterName,
            "/achievements",
            requestOptions,
            cancellationToken);

    public Task<CharacterProfessions> GetCharacterProfessionsAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<CharacterProfessions>(
            realmSlug,
            characterName,
            "/professions",
            requestOptions,
            cancellationToken);

    public Task<CharacterPvpSummary> GetCharacterPvpSummaryAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<CharacterPvpSummary>(
            realmSlug,
            characterName,
            "/pvp-summary",
            requestOptions,
            cancellationToken);

    public Task<MythicKeystoneProfile> GetCharacterMythicKeystoneProfileAsync(
        string realmSlug,
        string characterName,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetCharacterAsync<MythicKeystoneProfile>(
            realmSlug,
            characterName,
            "/mythic-keystone-profile",
            requestOptions,
            cancellationToken);

    public Task<Guild> GetGuildAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetGuildAsync<Guild>(realmSlug, guildSlug, string.Empty, requestOptions, cancellationToken);

    public Task<GuildRoster> GetGuildRosterAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetGuildAsync<GuildRoster>(realmSlug, guildSlug, "/roster", requestOptions, cancellationToken);

    public Task<GuildActivity> GetGuildActivityAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetGuildAsync<GuildActivity>(realmSlug, guildSlug, "/activity", requestOptions, cancellationToken);

    public Task<GuildAchievements> GetGuildAchievementsAsync(
        string realmSlug,
        string guildSlug,
        WowRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        GetGuildAsync<GuildAchievements>(realmSlug, guildSlug, "/achievements", requestOptions, cancellationToken);

    private Task<T> GetCharacterAsync<T>(
        string realmSlug,
        string characterName,
        string suffix,
        WowRequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(realmSlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterName);

        return _executor.GetAsync<T>(
            $"/profile/wow/character/{Uri.EscapeDataString(realmSlug)}/{Uri.EscapeDataString(characterName)}{suffix}",
            ProfileNamespace,
            requestOptions,
            null,
            cancellationToken);
    }

    private Task<T> GetGuildAsync<T>(
        string realmSlug,
        string guildSlug,
        string suffix,
        WowRequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(realmSlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(guildSlug);

        return _executor.GetAsync<T>(
            $"/data/wow/guild/{Uri.EscapeDataString(realmSlug)}/{Uri.EscapeDataString(guildSlug)}{suffix}",
            ProfileNamespace,
            requestOptions,
            null,
            cancellationToken);
    }
}
