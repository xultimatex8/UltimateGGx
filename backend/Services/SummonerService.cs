using backend.Data;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Enums;
using backend.Models.Riot;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class SummonerService
{
    private readonly AppDbContext _db;
    private readonly RiotApiService _riotApiService;

    public SummonerService(AppDbContext db, RiotApiService riotApiService)
    {
        _db = db;
        _riotApiService = riotApiService;
    }

    public async Task<SummonerDto> GetOrFetchSummonerAsync(string username, string tag, CancellationToken ct = default)
    {
        Summoner? existing = await _db.Summoners
            .Include(s => s.Queues)
            .FirstOrDefaultAsync(s => s.Username == username && s.Tag == tag, ct);

        if (existing is not null)
        {
            return MapSummonerToSummonerDto(existing);
        }

        AccountResponseDto account = await _riotApiService.GetRiotAccountAsync(username, tag, ct);
        SummonerResponseDto summonerInfo = await _riotApiService.GetRiotSummonerAsync(account.Puuid, ct);
        List<QueueResponseDto> queuesInfo = await _riotApiService.GetSummonerQueuesAsync(account.Puuid, ct);

        var summoner = new Summoner
        {
            Puuid = account.Puuid,
            Username = account.GameName,
            Tag = account.TagLine,
            Level = summonerInfo.SummonerLevel,
            ProfileIconId = summonerInfo.ProfileIconId,
            Queues = [.. queuesInfo.Select(q => new Queue
            {
                Type = MapQueueType(q.QueueType),
                Tier = q.Tier,
                Rank = q.Rank,
                Points = q.LeaguePoints,
                Wins = q.Wins,
                Losses = q.Losses
            })]
        };

        _db.Summoners.Add(summoner);
        await _db.SaveChangesAsync(ct);

        return MapSummonerToSummonerDto(summoner);
    }

    private static QueueType MapQueueType(string queueType) => queueType switch
    {
        "RANKED_SOLO_5x5" => QueueType.RANKED_SOLO,
        "RANKED_FLEX_SR" => QueueType.RANKED_FLEX,

        _ => throw new ArgumentOutOfRangeException(
            nameof(queueType),
            queueType,
            "Unknown queue type.")
    };

    private static SummonerDto MapSummonerToSummonerDto(Summoner summoner)
    {
        return new SummonerDto
        {
            Username = summoner.Username,
            Tag = summoner.Tag,
            Level = summoner.Level,
            ProfileIcondId = summoner.ProfileIconId,
            LastUpdate = summoner.UpdatedAt,
            Queues = [.. summoner.Queues.Select(q => MapQueueToQueueDto(q))]
        };
    }

    private static QueueDto MapQueueToQueueDto(Queue queue)
    {
        return new QueueDto
        {
            Type = queue.Type,
            Tier = queue.Tier,
            Rank = queue.Rank,
            Points = queue.Points,
            Wins = queue.Wins,
            Losses = queue.Losses
        };
    }
}