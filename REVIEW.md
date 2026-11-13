# WowClient Code Review

**Date:** 2026-11-14  
**Scope:** `src/WowClient`, tests, samples, packaging, and documentation.  
**Out of scope:** China (CN) support. CN is not planned, so it is only mentioned where the current API surface implies support that does not exist.

## Build and test status

- `dotnet build WowClient.slnx -c Release --nologo` — succeeds with **0 warnings, 0 errors**.
- `dotnet test WowClient.slnx -c Release --no-build --nologo` — **41/41 tests pass**.

The passing test suite does not catch the most important issue: the search tests assert the same incorrect contract that the implementation uses.

## Severity summary

| # | Severity | Area | Issue |
|---|----------|------|-------|
| 1 | High | Search API | Search request parameters, response fields, and result model do not match Blizzard's contract. |
| 2 | High | HTTP | `WowClientOptions.Timeout` does not apply to response body reads. |
| 3 | Medium | Resource ownership | Manual `WowClient.Create` can create undisposed `HttpClient` instances. |
| 4 | Medium | Raw requests | `GetRawAsync` defaults to the dynamic namespace, making the README item example fail. |
| 5 | Medium | Raw requests | `GetRawAsync` silently drops query strings embedded in `relativePath`. |
| 6 | Medium | Concurrency | Concurrent 401 responses can invalidate a newer token and trigger redundant OAuth requests. |
| 7 | Medium | Validation | Options validation only checks `Timeout`; invalid base URLs and retry settings are accepted. |
| 8 | Medium | Packaging | No license, license metadata, repository URL, or CI. |
| 9 | Low | Error handling | Malformed success bodies surface as raw `JsonException`; empty success bodies produce a `WowApiException` with a success status. |
| 10 | Low | Performance | JSON type metadata is resolved after the HTTP request instead of before it. |
| 11 | Low | Resource handling | Error-response body reads can leak an `HttpResponseMessage` if the read throws. |
| 12 | Low | Testability | Date-based `Retry-After` uses `DateTimeOffset.UtcNow` instead of the injected `TimeProvider`. |
| 13 | Low | API surface | `WowLocale.All` can be cast back to a mutable array. |
| 14 | Low | Sample | Exception handler does not cover transport/timeout exceptions and does not set `Retry-After` for 429s. |
| 15 | Low | Tests | Large model/endpoint surface is untested; the search fixture is fabricated. |
| 16 | Low | Docs/API | CN is still exposed and documented although it is explicitly unsupported. |

---

## High priority

### 1. Item search is implemented against the wrong Blizzard API contract

This affects `SearchItemsAsync`, `SearchAllItemsAsync`, the sample `/api/items?q=...` endpoint, and the tests.

#### Wrong request parameters

`src/WowClient/Endpoints/GameDataClient.cs:77`

```csharp
["q"] = query,
["page"] = page.ToString(CultureInfo.InvariantCulture),
["page_size"] = pageSize.ToString(CultureInfo.InvariantCulture),
```

Blizzard's search endpoints use field filters and underscored paging parameters. A real request looks like:

```text
/data/wow/search/item?namespace=static-us&name.en_US=Thunderfury&orderby=id&_page=1&_pageSize=100
```

There is no generic `q` parameter, and the documented paging names are `_page` and `_pageSize`, not `page` and `page_size`. The `locale` query parameter is also ignored by the search endpoint; localization is controlled by the field name, e.g. `name.en_US`.

#### Wrong response field mappings

`src/WowClient/Models/GameData/Search.cs:10`

```csharp
[JsonPropertyName("page_size")]
public int PageSize { get; init; }

[JsonPropertyName("max_page")]
public int MaxPage { get; init; }

[JsonPropertyName("total_count")]
public int TotalCount { get; init; }
```

Real search responses use camelCase fields:

```json
{
  "page": 1,
  "pageSize": 100,
  "maxPageSize": 100,
  "pageCount": 1000,
  "resultCountCapped": true,
  "results": []
}
```

`total_count` is not part of that payload. `MaxPage` should map to `pageCount`, and `maxPageSize` / `resultCountCapped` are missing from the model.

#### Wrong result data model

The search result `data` object is not always shaped like the detail item response. Real search results can contain localized name maps:

```json
"item_subclass": {
  "name": {
    "en_US": "Food & Drink",
    "de_DE": "Speisen und Getränke"
  }
}
```

The search implementation reuses `Item`, whose `WowName.Name` property is a `string`

`src/WowClient/Models/GameData/Items.cs:39`

```csharp
[JsonPropertyName("name")]
public string? Name { get; init; }
```

Deserializing a locale map into that property throws `JsonException`. Even after fixing request parameters and paging, search can still fail on real payloads.

#### Impact

- The requested search terms are ignored or misinterpreted.
- `PageSize`, `MaxPage`, and `TotalCount` deserialize as `0`.
- `SearchAllItemsAsync` stops after the first page because of `response.MaxPage <= 0` at `GameDataClient.cs:118`.
- If only the request parameters were fixed, paging would still be wrong.
- If only the response model were fixed, real search data could still fail to deserialize.
- The sample `/api/items?q=...` endpoint inherits the same behavior.

#### Why the tests do not catch it

The fixture and assertions encode the same incorrect contract:

- `tests/WowClient.Tests/Fixtures/item-search.json:2` uses `page_size`, `max_page`, and `total_count`.
- `tests/WowClient.Tests/RequestBuildingTests.cs:125` asserts `q`, `page`, and `page_size`.
- `tests/WowClient.Tests/DeserializationTests.cs:111` asserts the fabricated paging fields.

#### Suggested fix

- Send `name.{locale}`, `orderby=id`, `_page`, and `_pageSize`.
- Cap page size to the API's observed maximum (`maxPageSize: 100`), not `1000`.
- Map `pageSize`, `maxPageSize`, `pageCount`, and `resultCountCapped`.
- Use `PageCount` to terminate paging.
- Introduce a dedicated search-result DTO that can represent localized name maps instead of reusing the detail `Item` model.
- Replace the fabricated search fixture with a realistic recorded payload.
- Update the sample endpoint, README search documentation, and related tests.

---

### 2. `WowClientOptions.Timeout` does not cover response body reads

The executor uses `ResponseHeadersRead`:

`src/WowClient/Http/WowApiRequestExecutor.cs:81`

```csharp
var response = await client.SendAsync(
    request,
    HttpCompletionOption.ResponseHeadersRead,
    cancellationToken);
```

`HttpClient.Timeout` only covers the header phase. The body is read afterward:

`WowApiRequestExecutor.cs:36`

```csharp
await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
```

With the default `CancellationToken.None`, a server that sends headers and then stalls the body can hang indefinitely. This was verified with a local repro:

- `HttpClient.Timeout = 1s`
- `HttpCompletionOption.ResponseHeadersRead`
- body read takes 6 seconds

The body read completed after roughly 6 seconds instead of throwing. The token request uses the same pattern:

`src/WowClient/Auth/ClientCredentialsTokenProvider.cs:87` and `:105`

#### Suggested fix

Wrap the entire operation (headers plus body read/deserialization) in a linked `CancellationTokenSource` with `CancelAfter(options.Timeout)`, or use `ResponseContentRead` at the cost of buffering. Apply the same treatment to token requests.

---

## Medium priority

### 3. Manual construction can leak `HttpClient` instances

`src/WowClient/WowClient.cs:63`

```csharp
apiClient ??= new HttpClient { Timeout = options.Timeout };
authClient ??= new HttpClient { Timeout = options.Timeout };
```

`WowClient` is not `IDisposable`, so internally created clients have no disposal path. Repeated `WowClient.Create(...)` calls can leak connections and handlers. Callers that pass their own clients own those clients, but that contract is not documented.

#### Suggested fix

Implement `IDisposable`/`IAsyncDisposable` and dispose only internally created clients, or accept an `IHttpClientFactory`/handler and clearly document ownership.

---

### 4. `GetRawAsync` defaults to the dynamic namespace

`src/WowClient/WowClient.cs:92`

```csharp
_executor.GetRawAsync(
    relativePath,
    WowNamespaces.Dynamic(_options.Region),
    ...);
```

The README example calls:

`README.md:182`

```csharp
using var document = await client.GetRawAsync("/data/wow/item/19019");
```

That sends `namespace=dynamic-us`, but item endpoints require the static namespace. The documented example will fail. Profile raw paths are also defaulted to dynamic rather than profile.

#### Suggested fix

Require an explicit namespace for raw calls, or infer it from the path:

- `/profile/` → profile namespace
- `/data/wow/realm` and `/data/wow/connected-realm` → dynamic namespace
- other `/data/wow/` paths → static namespace

At minimum, correct the README example.

---

### 5. `GetRawAsync` drops query strings embedded in `relativePath`

`src/WowClient/Http/WowApiRequestExecutor.cs:184`

```csharp
var absolute = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + path, UriKind.Absolute);
```

Then at `:206`:

```csharp
var builder = new UriBuilder(absolute) { Query = string.Join("&", query) };
```

`UriBuilder.Query` replaces the existing query. Verified behavior:

```text
before: https://api.test/data/wow/item/19019?foo=bar
after:  https://api.test/data/wow/item/19019?namespace=static-us&locale=en_US
```

So a query string in the raw path is silently discarded. Passing values through `queryParameters` works, but the behavior is surprising for an escape-hatch API.

#### Suggested fix

Merge the existing query with the generated parameters, or explicitly document that `queryParameters` is the only supported way to pass query values.

---

### 6. Concurrent 401s can cause redundant token refreshes

`src/WowClient/Http/WowApiRequestExecutor.cs:85`

```csharp
if (response.StatusCode == HttpStatusCode.Unauthorized && !authRetried)
{
    authRetried = true;
    response.Dispose();
    _accessTokenProvider.InvalidateToken();
    continue;
}
```

`InvalidateToken()` unconditionally clears whatever token is current. If one request gets a 401 with an old token while another request has already stored a fresh token, the first request clears the fresh token and forces another OAuth request. Under load this causes token-refresh stampedes.

#### Suggested fix

Make invalidation conditional on the token used for the failed request. For example, change the contract to `InvalidateToken(string token)` and only clear the cached token when it matches.

---

### 7. Options validation is too narrow

`AddWowClient` validates only `Timeout`

`src/WowClient/Extensions/ServiceCollectionExtensions.cs:30`

and `WowClient.Create` does the same

`src/WowClient/WowClient.cs:58`

Not currently validated:

- `ApiBaseUrl` / `OAuthBaseUrl` being relative. These fail later in `BuildUri` via `AbsoluteUri`.
- `MaxRateLimitRetries < 0` and `MaxRateLimitDelay < 0`, which silently disable or alter retry behavior.
- Unsupported region values.
- Empty `UserAgent` (less critical).

#### Suggested fix

Add validators for absolute base URLs, non-negative retry settings, and supported regions. Apply the same checks in both DI registration and `WowClient.Create`.

---

### 8. Package and release readiness

`src/WowClient/WowClient.csproj:8` contains package metadata, but:

- There is no `LICENSE` file.
- There is no `PackageLicenseExpression` or `PackageLicenseFile`.
- There is no `RepositoryUrl` (only `RepositoryType`).
- There is no CI workflow under `.github/`.
- `README.md:220` explicitly notes the missing license.

The package version is `0.1.0`, while the default user agent is hardcoded to `WowClient/0.1.0` at `src/WowClient/WowClientOptions.cs:72`; these can drift.

#### Suggested fix

Add a license, package license metadata, repository URL, and a CI workflow. Derive the default user-agent version from the assembly version.

---

## Lower priority / polish

### 9. Malformed success bodies escape typed exceptions

A 200 OAuth response with invalid JSON throws `JsonException` at `src/WowClient/Auth/ClientCredentialsTokenProvider.cs:105` instead of `WowAuthenticationException`. Similarly, `GetAsync` and `GetRawAsync` deserialization failures surface as raw `JsonException`.

**Suggestion:** wrap deserialization failures in `WowApiException` / `WowAuthenticationException` with the original exception as `InnerException`.

### 10. Empty success body throws `WowApiException` with a success status

`src/WowClient/Http/WowApiRequestExecutor.cs:44` constructs a `WowApiException` with status 200/204 when the deserialized body is `null`.

**Suggestion:** use a clearer exception type/message, or treat an empty body as a distinct protocol error.

### 11. JSON type metadata is resolved after the network call

`src/WowClient/Http/WowApiRequestExecutor.cs:39` looks up `JsonTypeInfo<T>` only after receiving the response. An unregistered type therefore causes a wasted HTTP request.

**Suggestion:** resolve the type info before calling `SendAsync`.

### 12. Error-response body reads can leak a response

`src/WowClient/Http/WowApiRequestExecutor.cs:142` (`CreateApiExceptionAsync`) can throw while reading the body, leaving the `HttpResponseMessage` undisposed.

**Suggestion:** wrap the read in `try/finally` and dispose the response on failure.

### 13. Date-based `Retry-After` bypasses `TimeProvider`

`src/WowClient/Http/WowApiRequestExecutor.cs:128` uses `DateTimeOffset.UtcNow`, while token expiry uses the injected `TimeProvider`.

**Suggestion:** inject/use the same `TimeProvider` for retry-delay calculations so date-based `Retry-After` behavior is testable.

### 14. `WowLocale.All` is backed by a mutable array

`src/WowClient/WowLocale.cs:24` exposes `new[] { ... }` as `IReadOnlyList<string>`. Callers can cast it back to `string[]` and mutate it.

**Suggestion:** return `Array.AsReadOnly(...)` or `ImmutableArray`.

### 15. Sample exception handler gaps

`samples/WowClient.WebApi/Errors/WowApiExceptionHandler.cs:17` handles `WowApiException` and `WowAuthenticationException`, but not:

- `HttpRequestException` from transport failures.
- `TaskCanceledException` from timeouts.

A 429 is mapped to 503 at `:68` without a `Retry-After` header.

**Suggestion:** add handlers or fallbacks for transport/timeout exceptions, and propagate `Retry-After` where available.

### 16. Test coverage gaps

No tests currently cover:

- Timeout behavior, especially body-read stalls.
- Raw namespace defaults.
- Raw query preservation.
- Client disposal.
- Concurrent 401 token invalidation.
- Expanded options validation.
- Most of the model/endpoint surface: `CharacterStatus`, `CharacterMounts`, `CharacterAchievements`, `CharacterPvpSummary`, `MythicKeystoneProfile`, `GuildActivity`, `GuildAchievements`, item class/subclass, pet/mount indexes, spell/achievement media.

The search fixture and related tests are the most important to replace with realistic recorded payloads.

### 17. CN consistency

CN is explicitly out of scope, but `WowRegion.Cn` is still exposed and the README still advertises CN support. The resulting host will not work.

**Suggestion:** remove `WowRegion.Cn` or make it throw `NotSupportedException`, and remove `cn` from the README feature list.

---

## What looks good

- Clean Release build with warnings treated as errors.
- Source-generated `System.Text.Json` metadata and extension-data capture on models.
- Token caching with a semaphore, refresh skew, and a single retry on 401.
- Solid typed exception surface (`WowApiException`, `WowAuthenticationException`).
- DI registration is friendly to custom `IAccessTokenProvider` instances via `TryAdd`.
- Streaming deserialization and consistent `ConfigureAwait(false)` usage.
- Offline xUnit suite with realistic fixtures for most non-search endpoints.
- Sample API demonstrates DI, cancellation-token propagation, trimmed DTOs, and `ProblemDetails`.

---

## Suggested order of work

1. Fix the search request/response contract and replace the search fixture/tests; then update the sample and README.
2. Add a whole-operation timeout covering body reads.
3. Add disposal/ownership for internally created `HttpClient` instances.
4. Fix raw-request namespace defaults and preserve existing query strings.
5. Make 401 token invalidation conditional.
6. Expand options validation and package/release metadata.
7. Address the lower-priority polish items and expand test coverage.

---

## Appendix: verification notes

### Blizzard search contract

Verified against Blizzard's documented search API and real-world examples:

- Request parameters: `name.<locale>`, `orderby`, `_page`, `_pageSize`.
- Response fields: `page`, `pageSize`, `maxPageSize`, `pageCount`, `resultCountCapped`, `results`.
- Result `data` can include localized name maps, e.g. `item_subclass.name` keyed by locale.

### Timeout repro

A local repro used:

- `HttpClient.Timeout = 1s`
- `HttpCompletionOption.ResponseHeadersRead`
- a response body that takes 6 seconds to read

The body read completed after roughly 6 seconds instead of timing out, confirming that the configured timeout does not cover body reads.

### Raw query overwrite

A local repro confirmed that:

```csharp
var absolute = new Uri("https://api.test/data/wow/item/19019?foo=bar");
var builder = new UriBuilder(absolute) { Query = "namespace=static-us&locale=en_US" };
```

produces:

```text
https://api.test/data/wow/item/19019?namespace=static-us&locale=en_US
```

The original `foo=bar` query is lost.
