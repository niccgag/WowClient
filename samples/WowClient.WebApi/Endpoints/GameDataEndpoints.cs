using WowClient;
using WowClient.WebApi.Contracts;

namespace WowClient.WebApi.Endpoints;

public static class GameDataEndpoints
{
    public static IEndpointRouteBuilder MapGameDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("Game Data");

        group.MapGet("/realms", async (IWowClient wow, CancellationToken cancellationToken) =>
            {
                var index = await wow.GameData.GetRealmIndexAsync(cancellationToken: cancellationToken);
                var realms = index.Realms?.Select(RealmResponse.From).ToArray() ?? [];
                return TypedResults.Ok(realms);
            })
            .WithName("GetRealms")
            .WithSummary("Lists all retail realms.");

        group.MapGet("/realms/{slug}", async (
                string slug,
                IWowClient wow,
                CancellationToken cancellationToken) =>
            {
                var realm = await wow.GameData.GetRealmAsync(slug, cancellationToken: cancellationToken);
                return TypedResults.Ok(RealmResponse.From(realm));
            })
            .WithName("GetRealm")
            .WithSummary("Gets one realm by slug.");

        group.MapGet("/items/{id:int}", async (
                int id,
                IWowClient wow,
                CancellationToken cancellationToken) =>
            {
                var item = await wow.GameData.GetItemAsync(id, cancellationToken: cancellationToken);
                return TypedResults.Ok(ItemResponse.From(item));
            })
            .WithName("GetItem")
            .WithSummary("Gets an item by ID.");

        group.MapGet("/items", SearchItemsAsync)
            .WithName("SearchItems")
            .WithSummary("Searches items by name.");

        group.MapGet("/spells/{id:int}", async (
                int id,
                IWowClient wow,
                CancellationToken cancellationToken) =>
            {
                var spell = await wow.GameData.GetSpellAsync(id, cancellationToken: cancellationToken);
                return TypedResults.Ok(SpellResponse.From(spell));
            })
            .WithName("GetSpell")
            .WithSummary("Gets a spell by ID.");

        group.MapGet("/mounts", async (IWowClient wow, CancellationToken cancellationToken) =>
            {
                var index = await wow.GameData.GetMountIndexAsync(cancellationToken: cancellationToken);
                var mounts = index.Mounts?.Select(MountResponse.From).ToArray() ?? [];
                return TypedResults.Ok(mounts);
            })
            .WithName("GetMounts")
            .WithSummary("Lists every mount.");

        return app;
    }

    private static async Task<IResult> SearchItemsAsync(
        string? q,
        int? page,
        int? pageSize,
        IWowClient wow,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid search request",
                detail: "The 'q' query parameter is required.");
        }

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 50;
        if (currentPage < 1 || currentPageSize is < 1 or > 100)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid search request",
                detail: "'page' must be at least 1 and 'pageSize' must be between 1 and 100.");
        }

        var response = await wow.GameData.SearchItemsAsync(
            q,
            currentPage,
            currentPageSize,
            cancellationToken: cancellationToken);

        return Results.Ok(ItemSearchResponse.From(response));
    }
}
