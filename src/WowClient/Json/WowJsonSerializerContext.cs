using System.Text.Json.Serialization;
using WowClient.Auth;
using WowClient.Models;
using WowClient.Models.GameData;
using WowClient.Models.Profile;

namespace WowClient.Json;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(WowErrorResponse))]
[JsonSerializable(typeof(OAuthErrorResponse))]
[JsonSerializable(typeof(Realm))]
[JsonSerializable(typeof(RealmIndex))]
[JsonSerializable(typeof(ConnectedRealm))]
[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(Spell))]
[JsonSerializable(typeof(Mount))]
[JsonSerializable(typeof(MountIndex))]
[JsonSerializable(typeof(Pet))]
[JsonSerializable(typeof(PetIndex))]
[JsonSerializable(typeof(Achievement))]
[JsonSerializable(typeof(AchievementCategoryIndex))]
[JsonSerializable(typeof(ItemClassIndex))]
[JsonSerializable(typeof(ItemClass))]
[JsonSerializable(typeof(ItemSubclass))]
[JsonSerializable(typeof(WowSearchResponse<ItemSearchResult>))]
[JsonSerializable(typeof(WowMedia))]
[JsonSerializable(typeof(CharacterProfile))]
[JsonSerializable(typeof(CharacterStatus))]
[JsonSerializable(typeof(CharacterEquipment))]
[JsonSerializable(typeof(CharacterMounts))]
[JsonSerializable(typeof(CharacterAchievements))]
[JsonSerializable(typeof(CharacterProfessions))]
[JsonSerializable(typeof(CharacterPvpSummary))]
[JsonSerializable(typeof(MythicKeystoneProfile))]
[JsonSerializable(typeof(Guild))]
[JsonSerializable(typeof(GuildRoster))]
[JsonSerializable(typeof(GuildActivity))]
[JsonSerializable(typeof(GuildAchievements))]
internal sealed partial class WowJsonSerializerContext : JsonSerializerContext
{
}
