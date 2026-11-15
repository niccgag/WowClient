using WowClient;
using WowClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Bind the client options from the "Blizzard" configuration section.
builder.Services.AddWowClient(options =>
    builder.Configuration.GetSection("Blizzard").Bind(options));

var app = builder.Build();

var api = app.MapGroup("/api");

api.MapGet("/realms", async (IWowClient wow, CancellationToken cancellationToken) =>
    await wow.GameData.GetRealmIndexAsync(cancellationToken: cancellationToken));

api.MapGet("/realms/{slug}", async (string slug, IWowClient wow, CancellationToken cancellationToken) =>
    await wow.GameData.GetRealmAsync(slug, cancellationToken: cancellationToken));

api.MapGet("/items/{id:int}", async (int id, IWowClient wow, CancellationToken cancellationToken) =>
    await wow.GameData.GetItemAsync(id, cancellationToken: cancellationToken));

api.MapGet("/items", async (string q, IWowClient wow, CancellationToken cancellationToken) =>
    await wow.GameData.SearchItemsAsync(q, cancellationToken: cancellationToken));

api.MapGet("/spells/{id:int}", async (int id, IWowClient wow, CancellationToken cancellationToken) =>
    await wow.GameData.GetSpellAsync(id, cancellationToken: cancellationToken));

api.MapGet("/characters/{realm}/{name}", async (
        string realm,
        string name,
        IWowClient wow,
        CancellationToken cancellationToken) =>
    await wow.Profile.GetCharacterProfileAsync(realm, name, cancellationToken: cancellationToken));

api.MapGet("/guilds/{realm}/{guild}/roster", async (
        string realm,
        string guild,
        IWowClient wow,
        CancellationToken cancellationToken) =>
    await wow.Profile.GetGuildRosterAsync(realm, guild, cancellationToken: cancellationToken));

app.Run();
