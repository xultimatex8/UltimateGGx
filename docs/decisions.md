# Decision Log — UltimateGGx

Design and technical decisions made. Newest entries at the top.

---

## Include participant details in match list responses

**Decision:** each `MatchDto` returned by the match history endpoint includes the complete list of `ParticipantDetailDto`s, even though the frontend initially displays only summary information. No additional endpoint is provided to fetch match details when a match is expanded.

**Alternatives considered:**

*How to expose participant details:*
- *Return only match summaries and fetch participant details on demand* — reduces the size of the initial response, but requires an additional HTTP request whenever the user expands a match, increasing latency and adding complexity to both the frontend and backend.
- *Include participant details in every `MatchDto` (chosen)* — slightly increases the response payload, but avoids extra requests, simplifies the frontend, and provides an immediate expansion experience.

**Why:** the match history endpoint is paginated and returns only 10 matches at a time, making the additional payload of participant details relatively small. Since the backend already has the data available when constructing the response, including it incurs no additional database queries. Returning all information in a single response keeps the API simpler and avoids introducing a dedicated match-details endpoint before it is justified by actual performance requirements.

---

## Lazy synchronization of match details

**Decision:** on the first synchronization for a given queue type, only the latest 10 matchIds for that queue are stored, each represented by a lightweight `MatchReference` entity linked to the summoners it was discovered for. Detailed match information is fetched only for the 10 most recent matches shown to the user, at which point a full `Match` entity is created and linked back to its `MatchReference`. Whether a match has been synchronized is determined by checking whether `MatchReference.Match` is null, rather than a separate flag. Older matches are synchronized on demand when the user navigates to them.

**Alternatives considered:**

*How many matches to fetch initially:*
- *Fetch the latest 20 matches regardless of queue* — provides broader history, but stores references for matches that may never be requested and mixes different queue types.
- *Fetch the latest 10 matches for the requested queue type (chosen)* — aligns the initial synchronization with the requested game mode (Draft Pick, Ranked Solo, or Ranked Flex), reduces Riot API usage, and avoids storing references for unrelated queues.

*How to represent partially synchronized matches:*
- *Require all match fields to exist before persisting* — forces fetching the full match payload immediately, preventing incremental synchronization.
- *Make `Match` fields optional and populate them incrementally* — allows a `Match` row to exist before its detail is fetched, but forces every consumer of `Match` to treat fields such as `Teams`, `EndOfGameResult`, or `GameDuration` as potentially absent even after synchronization is complete, since the type alone no longer guarantees a match is fully populated.
- *Introduce a separate `MatchReference` entity holding only `MatchId`, many-to-many with `Summoner` (chosen)* — keeps `Match` as an all-or-nothing entity: a row only exists once fully populated, so every field can remain non-nullable and every consumer can rely on that guarantee. `MatchReference` absorbs the "discovered but not yet detailed" state instead, and also correctly models that the same match can be discovered through multiple summoners' histories without duplicating the `matchId` or re-fetching it once any one summoner has already triggered its discovery.

*How to track whether a match's details have been fetched:*
- *Add a `DetailsFetched` boolean flag on `MatchReference`* — explicit, but introduces a second source of truth alongside the `Match` navigation property; the two could fall out of sync (e.g. a flag left `true` after the linked `Match` is deleted).
- *Check whether `MatchReference.Match` is null (chosen)* — a single source of truth: a match is considered synchronized exactly when its `Match` navigation is populated, with no separate state to keep consistent.

**Why:** Riot's Match-V5 API is rate limited, and each match detail requires a separate request. Storing all discovered `matchId`s via `MatchReference` while synchronizing only the matches the user actually views significantly reduces API consumption without losing the ability to synchronize older matches later. Separating the "discovered" and "detailed" states into two entities also avoids weakening `Match`'s guarantees — once a `Match` row exists, its data is always complete, keeping the rest of the domain free of defensive null-checks for a state that is otherwise temporary. Deriving synchronization status from the `Match` link itself, rather than a separate flag, avoids a redundant piece of state that could drift out of sync.

---

## Extract Data Dragon synchronization logic into a dedicated service

**Decision:** extract the version checking and synchronization logic from `DataDragonSyncBackgroundService` into a dedicated `DataDragonSyncCheckerService`. The background service is now only responsible for scheduling periodic executions, while the checker encapsulates the synchronization logic behind an `IDataDragonSyncCheckerService` interface.

**Alternatives considered:**

*How to organize the synchronization logic:*
- *Keep the synchronization logic as a private method inside the background service* — simple implementation, but makes the synchronization behaviour difficult to test in isolation.
- *Expose the private method using `internal` and `InternalsVisibleTo`* — allows tests to invoke the method directly, but weakens encapsulation solely for testing purposes.
- *Extract the synchronization logic into a dedicated service (chosen)* — separates scheduling from business logic, provides a public interface that can be tested directly, and follows the Single Responsibility Principle.

*How to test the synchronization behaviour:*
- *Test only the background service through `ExecuteAsync`* — impractical because it contains an infinite loop driven by `PeriodicTimer`, making tests complex and tightly coupled to infrastructure.
- *Test the extracted synchronization service directly (chosen)* — allows straightforward unit tests by mocking dependencies without interacting with timers, hosted services, or service scopes.

**Why:** the background service's responsibility is scheduling work, not implementing synchronization logic. Extracting the synchronization behaviour into its own service improves separation of concerns, makes the business logic independently testable without exposing internal methods, and results in a cleaner architecture where infrastructure and application logic are clearly separated.

---

## Separate HTTP clients for Riot platform and regional APIs

**Decision:** register two dedicated `HttpClient` instances: one targeting the Riot platform routing (`https://euw1.api.riotgames.com/`) and another targeting the Riot regional routing (`https://europe.api.riotgames.com/`). Services resolve the appropriate client depending on the endpoint being called.

**Alternatives considered:**

*How to handle Riot's routing model:*
- *Single `HttpClient` with a fixed base address* — works only for one routing domain and requires manually constructing absolute URLs for endpoints on the other domain.
- *Single `HttpClient` that changes its `BaseAddress` dynamically* — complicates the implementation and makes the client stateful.
- *Two dedicated named `HttpClient`s (chosen)* — clearly separates platform and regional endpoints, keeps services simple, and aligns with Riot's API routing model.

*How to configure HTTP clients:*
- *Register one typed `HttpClient` per service* — suitable when a service communicates with a single external API, but Riot requires two different base URLs depending on the endpoint.
- *Register named `HttpClient`s (chosen)* — allows services to resolve the correct client for each request while sharing common configuration such as the Riot API key.

**Why:** Riot's APIs are split between platform-routed endpoints (e.g. `euw1.api.riotgames.com`) and regional-routed endpoints (e.g. `europe.api.riotgames.com`). Using dedicated named `HttpClient`s avoids hardcoded absolute URLs, keeps endpoint selection explicit, and follows the routing model defined by Riot.

---

## Periodic sync of the champion/summoner spell catalog

**Decision:** a `BackgroundService` checks Data Dragon's latest version on startup and every 6 hours after that using a `PeriodicTimer`, re-syncing `Champion`/`SummonerSpell` only when the version changed.

**Alternatives considered:**

*When/where to sync:*
- *In-process `BackgroundService`, checking every 6 hours (chosen)* — self-heals with no manual step, without polling a static CDN needlessly.
- *Manual sync endpoint only* — relies on someone remembering to call it after every patch; a missed patch silently leaves the catalog stale.
- *Sync inline on every request needing catalog data* — couples unrelated request latency to an external service, and checks far more often than needed.
- *External cron job hitting an endpoint* — adds an unnecessary external dependency for something .NET already solves in-process.

*How to implement the periodic loop:*
- *`Thread.Sleep(interval)` in a loop* — blocks a real OS thread for the entire wait, wasting a thread for hours at a time. Fully synchronous.
- *`while (...) { work(); await Task.Delay(interval); }`* — async and non-blocking, works fine, but the wait only starts *after* the previous run finishes, so intervals can drift slightly over time.
- *`PeriodicTimer` (chosen)* — .NET-native primitive built for this case: async, non-blocking, no drift, integrates cleanly with `CancellationToken` for graceful shutdown.
- *Quartz.NET / Hangfire* — full scheduling libraries (cron expressions, persistence, retries, dashboards). Overkill for a single 6-hourly check; would add a dependency that is not needed.

**Why:** patches ship every ~2 weeks; a 6h in-process check via `BackgroundService` + `PeriodicTimer` is frequent enough to pick one up same-day without polling a static CDN needlessly, self-heals with no manual step, and doesn't need an external scheduling dependency.