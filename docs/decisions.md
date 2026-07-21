# Decision Log

Design and technical decisions made. Newest entries at the top.

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