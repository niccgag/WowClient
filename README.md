# WowClient

An asynchronous .NET client for the [Blizzard World of Warcraft APIs](https://develop.battle.net/documentation/world-of-warcraft).

The library covers the WoW **Game Data** and **Profile** APIs, handles the OAuth client-credentials flow
automatically, caches tokens, retries once after an unauthorized response, and offers both dependency
injection and manual construction.

## Requirements

- .NET 10 SDK or runtime
- A Battle.net API client from <https://develop.battle.net/access/clients>

## Features

- Game Data: realms, connected realms, items, item media/search, spells, mounts, battle pets,
  achievements, achievement categories, and item classes/subclasses.
- Profile: character summary, status, equipment, mounts, achievements, professions, PvP summary,
  mythic keystone profile, and guild summary/roster/activity/achievements.
- Automatic OAuth 2.0 client-credentials token acquisition with in-memory caching and expiry refresh.
- Single retry after HTTP 401 by refreshing the token, and optional `Retry-After` handling for HTTP 429.
- Region-aware hosts and namespaces (`us`, `eu`, `kr`, `tw`) with per-request overrides.
- `System.Text.Json` source-generated models for trimming/AOT-friendly serialization.
- Typed exceptions that preserve Blizzard error codes, types, details, and the raw response body.
- xUnit test suite that runs entirely offline against realistic fixtures.

## Quick start (dependency injection)

```csharp
using WowClient;
using WowClient.Extensions;

builder.Services.AddWowClient(options =>
{
    options.ClientId = configuration["Blizzard:ClientId"]!;
    options.ClientSecret = configuration["Blizzard:ClientSecret"]!;
    options.Region = WowRegion.Eu;
    options.Locale = WowLocale.EnglishGb;
});
```

Then inject `IWowClient`:

```csharp
public sealed class ItemService(IWowClient wow)
{
    public async Task<string?> GetItemNameAsync(int itemId, CancellationToken cancellationToken)
    {
        var item = await wow.GameData.GetItemAsync(itemId, cancellationToken: cancellationToken);
        return item.Name;
    }
}
```

## Quick start (manual construction)

```csharp
using WowClient;

using var client = WowClient.Create(new WowClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("BLIZZARD_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("BLIZZARD_CLIENT_SECRET")!,
    Region = WowRegion.Us,
});

var realm = await client.GameData.GetRealmAsync("tarren-mill");
var character = await client.Profile.GetCharacterProfileAsync("tarren-mill", "Example");
```

`WowClient.Create` disposes HTTP clients it creates. If you pass your own `HttpClient` instances, you
remain responsible for disposing them.

## Sample web API

[`samples/WowClient.WebApi`](https://github.com/niccgag/WowClient/tree/main/samples/WowClient.WebApi) is a deliberately small ASP.NET Core API. Everything lives in
[`Program.cs`](https://github.com/niccgag/WowClient/blob/main/samples/WowClient.WebApi/Program.cs):
options binding, `AddWowClient`, and a few minimal API endpoints that call `IWowClient` directly.

```csharp
builder.Services.AddWowClient(options =>
    builder.Configuration.GetSection("Blizzard").Bind(options));

var app = builder.Build();

app.MapGet("/items/{id:int}", async (int id, IWowClient wow, CancellationToken cancellationToken) =>
    await wow.GameData.GetItemAsync(id, cancellationToken: cancellationToken));

app.Run();
```

The sample returns the library models directly to keep the focus on client usage. A production API
would usually project them into its own response contracts. Every handler forwards
`CancellationToken`, and `IWowClient` is thread-safe and registered as a singleton.

Configure credentials with user-secrets (recommended for local development):

```bash
dotnet user-secrets --project samples/WowClient.WebApi set "Blizzard:ClientId" "<client-id>"
dotnet user-secrets --project samples/WowClient.WebApi set "Blizzard:ClientSecret" "<client-secret>"
dotnet run --project samples/WowClient.WebApi
```

Or with environment variables:

```bash
Blizzard__ClientId=... Blizzard__ClientSecret=... dotnet run --project samples/WowClient.WebApi
```

| Method | Route |
| --- | --- |
| GET | `/api/realms` |
| GET | `/api/realms/{slug}` |
| GET | `/api/items/{id}` |
| GET | `/api/items?q={query}` |
| GET | `/api/spells/{id}` |
| GET | `/api/characters/{realm}/{name}` |
| GET | `/api/guilds/{realm}/{guild}/roster` |

## Configuration

| Option | Default | Purpose |
| --- | --- | --- |
| `ClientId` | empty | Battle.net API client ID. |
| `ClientSecret` | empty | Battle.net API client secret. Prefer environment variables or user-secrets. |
| `Region` | `Us` | Resolves the API host and default namespace. |
| `Locale` | `en_US` | Default `locale` query value. |
| `ApiBaseUrl` | `https://{region}.api.blizzard.com` | Override for testing or a proxy. Must be an absolute `https` URL without a query, fragment, or user info (`http` is accepted for loopback addresses only). |
| `OAuthBaseUrl` | `https://oauth.battle.net` | Override for testing. Same URL rules as `ApiBaseUrl`. |
| `Timeout` | 30 seconds | Timeout for the entire HTTP operation, including response body reads. |
| `UserAgent` | `WowClient/{version}` | User-Agent sent with requests. The version is derived from the library assembly. Blizzard recommends identifying your application. |
| `MaxRateLimitRetries` | `1` | Retries allowed for HTTP 429 responses. |
| `MaxRateLimitDelay` | 5 seconds | Longest `Retry-After` the client will wait before surfacing the error. |

### Namespace and locale overrides

Every endpoint accepts optional `WowRequestOptions`:

```csharp
var item = await client.GameData.GetItemAsync(
    19019,
    new WowRequestOptions
    {
        Namespace = "static-classic-us",
        Locale = WowLocale.GermanDe,
    });
```

## Endpoints

### Game Data (`client.GameData`)

| Method | Endpoint |
| --- | --- |
| `GetRealmIndexAsync` | `/data/wow/realm/index` |
| `GetRealmAsync` | `/data/wow/realm/{slug}` |
| `GetConnectedRealmAsync` | `/data/wow/connected-realm/{id}` |
| `GetItemAsync` | `/data/wow/item/{id}` |
| `GetItemMediaAsync` | `/data/wow/media/item/{id}` |
| `SearchItemsAsync` / `SearchAllItemsAsync` | `/data/wow/search/item` |
| `GetSpellAsync` | `/data/wow/spell/{id}` |
| `GetSpellMediaAsync` | `/data/wow/media/spell/{id}` |
| `GetMountIndexAsync` / `GetMountAsync` | `/data/wow/mount/...` |
| `GetPetIndexAsync` / `GetPetAsync` | `/data/wow/pet/...` |
| `GetAchievementAsync` | `/data/wow/achievement/{id}` |
| `GetAchievementMediaAsync` | `/data/wow/media/achievement/{id}` |
| `GetAchievementCategoryIndexAsync` | `/data/wow/achievement-category/index` |
| `GetItemClassIndexAsync` | `/data/wow/item-class/index` |
| `GetItemClassAsync` | `/data/wow/item-class/{id}` |
| `GetItemSubclassAsync` | `/data/wow/item-class/{classId}/item-subclass/{subclassId}` |

### Item search

`SearchItemsAsync` follows Blizzard's search contract: it sends `name.{locale}` (for example
`name.en_US`), `orderby=id`, `_page`, and `_pageSize`. Page size is limited client-side to 100;
larger values throw `ArgumentOutOfRangeException` before a request is sent. `MaxPageSize` reports the
largest page size the API accepted for that specific query, which can be lower. Responses expose
`Page`, `PageSize`, `MaxPageSize`, `PageCount`, and `ResultCountCapped`, and
`SearchAllItemsAsync` follows pages until `PageCount` is reached. Search result names are locale maps;
use `WowLocalizedName.EnglishUs`, `Get(locale)`, or the indexer.

```csharp
var page = await client.GameData.SearchItemsAsync("Thunderfury", page: 1, pageSize: 100);
foreach (var result in page.Results ?? [])
{
    Console.WriteLine(result.Data?.Name?.EnglishUs);
}
```

### Profile (`client.Profile`)

| Method | Endpoint |
| --- | --- |
| `GetCharacterProfileAsync` | `/profile/wow/character/{realm}/{name}` |
| `GetCharacterStatusAsync` | `.../status` |
| `GetCharacterEquipmentAsync` | `.../equipment` |
| `GetCharacterMountsAsync` | `.../collections/mounts` |
| `GetCharacterAchievementsAsync` | `.../achievements` |
| `GetCharacterProfessionsAsync` | `.../professions` |
| `GetCharacterPvpSummaryAsync` | `.../pvp-summary` |
| `GetCharacterMythicKeystoneProfileAsync` | `.../mythic-keystone-profile` |
| `GetGuildAsync` | `/data/wow/guild/{realm}/{guild}` |
| `GetGuildRosterAsync` | `.../roster` |
| `GetGuildActivityAsync` | `.../activity` |
| `GetGuildAchievementsAsync` | `.../achievements` |

Anything not covered can be requested through the raw escape hatch:

```csharp
using var document = await client.GetRawAsync("/data/wow/item/19019");
```

The namespace is inferred from the path: `/profile/...` and `/data/wow/guild/...` use the profile
namespace (for example `/data/wow/guild/tarren-mill/example`), realm and connected-realm paths use the
dynamic namespace, and other `/data/wow/...` paths — including `/data/wow/guild-crest/...` — use the
static namespace. Pass `WowRequestOptions.Namespace` to override it. Query strings embedded in the
path are preserved and merged with `namespace`, `locale`, and any `queryParameters`; explicit
`WowRequestOptions.Namespace`/`Locale` win over `queryParameters`, which win over embedded values.

The caller owns and must dispose the returned `JsonDocument`.

## Error handling

- `WowApiException` is thrown for non-success API responses and exposes `StatusCode`, `ErrorCode`,
  `ErrorType`, `ErrorDetail`, `RequestUri`, `ResponseBody`, and `RetryAfter`.
- `WowAuthenticationException` is thrown when credentials are missing or the OAuth endpoint rejects them.
- `WowProtocolException` is thrown when a successful response cannot be interpreted, such as an empty
  body where a document was expected.
- Malformed JSON in a successful response is wrapped in `WowApiException` (or
  `WowAuthenticationException` for token responses) with the original `JsonException` as the inner
  exception.
- A 401 triggers one token refresh and retry. A second 401 is surfaced as `WowApiException`.
- A 429 is retried when `Retry-After` is present, no longer than `MaxRateLimitDelay`, retries remain,
  and the delay fits within the remaining operation timeout; otherwise the 429 `WowApiException`
  (including `RetryAfter`) is surfaced instead of a timeout.

## Testing

```bash
dotnet test
```

The library tests use a fake `HttpMessageHandler` and recorded JSON fixtures, so they need no
credentials or network access. Coverage includes token caching/refresh, conditional token
invalidation, whole-operation timeouts, raw request namespace inference and query merging, disposal
ownership, URL and namespace construction, options validation, region handling, search paging,
deserialization across the endpoint surface, rate limiting, authentication errors, and DI
registration.

## Project layout

```
src/WowClient                      Library
tests/WowClient.Tests              xUnit test suite
samples/WowClient.WebApi           ASP.NET Core sample API
```

## Scope and limitations

- Targets retail WoW namespaces. Classic variants are available through `WowRequestOptions.Namespace`
  (for example `static-classic-us`), but dedicated Classic models are out of scope for v0.1.
- The endpoint list is a curated subset of the full Blizzard catalog, chosen for common use cases.
- No built-in request throttling; Blizzard publishes rate limits (about 36,000 requests/hour and
  100 requests/second per client). Use `Retry-After` handling and your own caching for high-volume use.

## License

MIT. See [LICENSE](https://github.com/niccgag/WowClient/blob/main/LICENSE).
