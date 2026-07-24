# Decision Log — UltimateGGx

Design and technical decisions made. Newest entries at the top.

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