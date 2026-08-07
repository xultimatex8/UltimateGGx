using backend.Data;
using backend.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Enums;
using backend.Models.Riot;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class SummonerService : ISummonerService
{
    private readonly AppDbContext _db;
    private readonly IRiotApiService _riotApiService;

    public SummonerService(AppDbContext db, IRiotApiService riotApiService)
    {
        _db = db;
        _riotApiService = riotApiService;
    }

    public async Task<SummonerDto> GetOrFetchSummonerAsync(string username, string tag, CancellationToken ct = default)
    {
        Summoner? existing = await _db.Summoners
            .Include(s => s.Queues)
            .FirstOrDefaultAsync(s => s.Username == username && s.Tag == tag, ct);

        if (existing is null)
        {
            return await SyncSummonerAsync(username, tag, ct);
        }

        return MapSummonerToSummonerDto(existing);
    }

    public async Task<SummonerDto> SyncSummonerByPuuidAsync(string puuid, CancellationToken ct = default)
    {
        AccountResponseDto account = await _riotApiService.GetRiotAccountByPuuidAsync(puuid, ct);

        return await SyncSummonerAsync(account.GameName, account.TagLine, ct);
    }

    public async Task<SummonerDto> SyncSummonerAsync(string username, string tag, CancellationToken ct = default)
    {
        AccountResponseDto account = await _riotApiService.GetRiotAccountAsync(username, tag, ct);
        SummonerResponseDto summonerInfo = await _riotApiService.GetRiotSummonerAsync(account.Puuid, $"{username}#{tag}", ct);
        List<QueueResponseDto> queuesInfo = await _riotApiService.GetSummonerQueuesAsync(account.Puuid, ct);

        Summoner? summoner = await _db.Summoners
            .Include(s => s.Queues)
            .FirstOrDefaultAsync(s => s.Puuid == account.Puuid, ct);

        if (summoner is null)
        {
            summoner = new Summoner
            {
                Puuid = account.Puuid
            };

            _db.Summoners.Add(summoner);
        }
        else
        {
            _db.Queues.RemoveRange(summoner.Queues);
        }

        summoner.Username = account.GameName;
        summoner.Tag = account.TagLine;
        summoner.Level = summonerInfo.SummonerLevel;
        summoner.ProfileIconId = summonerInfo.ProfileIconId;

        summoner.Queues =
        [
            .. queuesInfo
                .Select(q => new
                {
                    Queue = q,
                    Type = TryMapQueueType(q.QueueType)
                })
                .Where(x => x.Type.HasValue)
                .Select(x => new Queue
                {
                    Type = x.Type!.Value,
                    Tier = x.Queue.Tier,
                    Rank = x.Queue.Rank,
                    Points = x.Queue.LeaguePoints,
                    Wins = x.Queue.Wins,
                    Losses = x.Queue.Losses
                })
        ];

        await _db.SaveChangesAsync(ct);

        return MapSummonerToSummonerDto(summoner);
    }

    private static QueueType? TryMapQueueType(string queueType) => queueType switch
    {
        "RANKED_SOLO_5x5" => QueueType.RANKED_SOLO,
        "RANKED_FLEX_SR" => QueueType.RANKED_FLEX,

        _ => null
    };

    private static SummonerDto MapSummonerToSummonerDto(Summoner summoner)
    {
        return new SummonerDto
        {
            Puuid = summoner.Puuid,
            Username = summoner.Username,
            Tag = summoner.Tag,
            Level = summoner.Level,
            ProfileIconId = summoner.ProfileIconId,
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