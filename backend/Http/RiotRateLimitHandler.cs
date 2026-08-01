using System.Threading.RateLimiting;

namespace backend.Http;

public class RiotRateLimitHandler : DelegatingHandler
{
    private static readonly RateLimiter PerSecond = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
    {
        PermitLimit = GetEnvInt("RIOT_RATE_LIMIT_PER_SECOND", 20),
        Window = TimeSpan.FromSeconds(GetEnvInt("RIOT_RATE_LIMIT_PER_SECOND_WINDOW", 1)),
        QueueLimit = int.MaxValue,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
    });

    private static readonly RateLimiter PerWindow = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
    {
        PermitLimit = GetEnvInt("RIOT_RATE_LIMIT_PER_WINDOW", 100),
        Window = TimeSpan.FromMinutes(GetEnvInt("RIOT_RATE_LIMIT_WINDOW_MINUTES", 2)),
        QueueLimit = int.MaxValue,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
    });

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        using RateLimitLease lease1 = await PerSecond.AcquireAsync(1, ct);
        using RateLimitLease lease2 = await PerWindow.AcquireAsync(1, ct);

        return await base.SendAsync(request, ct);
    }

    private static int GetEnvInt(string key, int fallback)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        return int.TryParse(value, out int parsed) ? parsed : fallback;
    }
}