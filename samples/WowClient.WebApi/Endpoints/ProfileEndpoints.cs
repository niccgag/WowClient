using WowClient;
using WowClient.WebApi.Contracts;

namespace WowClient.WebApi.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("Profile");

        group.MapGet("/characters/{realm}/{name}", async (
                string realm,
                string name,
                IWowClient wow,
                CancellationToken cancellationToken) =>
            {
                var profile = await wow.Profile.GetCharacterProfileAsync(
                    realm,
                    name,
                    cancellationToken: cancellationToken);
                return TypedResults.Ok(CharacterResponse.From(profile));
            })
            .WithName("GetCharacter")
            .WithSummary("Gets a character profile summary.");

        group.MapGet("/characters/{realm}/{name}/equipment", async (
                string realm,
                string name,
                IWowClient wow,
                CancellationToken cancellationToken) =>
            {
                var equipment = await wow.Profile.GetCharacterEquipmentAsync(
                    realm,
                    name,
                    cancellationToken: cancellationToken);
                return TypedResults.Ok(CharacterEquipmentResponse.From(equipment));
            })
            .WithName("GetCharacterEquipment")
            .WithSummary("Gets a character's equipped items.");

        group.MapGet("/guilds/{realm}/{guild}/roster", async (
                string realm,
                string guild,
                IWowClient wow,
                CancellationToken cancellationToken) =>
            {
                var roster = await wow.Profile.GetGuildRosterAsync(
                    realm,
                    guild,
                    cancellationToken: cancellationToken);
                return TypedResults.Ok(GuildRosterResponse.From(roster));
            })
            .WithName("GetGuildRoster")
            .WithSummary("Gets a guild roster.");

        return app;
    }
}
