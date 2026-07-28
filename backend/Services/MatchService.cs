using backend.Data;
using backend.Exceptions;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Enums;
using backend.Models.Riot;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class MatchService : IMatchService
{
    private readonly AppDbContext _db;
    private readonly IRiotApiService _riotApiService;

    public MatchService(AppDbContext db, IRiotApiService riotApiService)
    {
        _db = db;
        _riotApiService = riotApiService;
    }

    public async Task FetchSummonerMatchesAsync(string puuid, QueueType queueType = QueueType.DRAFT_PICK, CancellationToken ct = default)
    {
        List<string> matchIds = await _riotApiService.GetSummonerMatchesAsync(puuid, queueType, ct);

        Summoner summoner = await _db.Summoners
            .Include(s => s.MatchReferences)
            .FirstOrDefaultAsync(s => s.Puuid == puuid, ct)
            ?? throw new NotFoundException(nameof(Summoner), nameof(Summoner.Puuid), puuid);

        List<MatchReference> existingRefs = await _db.MatchReferences
            .Where(m => matchIds.Contains(m.MatchId))
            .ToListAsync(ct);

        HashSet<string> summonerExistingIds = [.. summoner.MatchReferences.Select(m => m.MatchId)];

        foreach (string matchId in matchIds)
        {
            MatchReference? reference = existingRefs.FirstOrDefault(m => m.MatchId == matchId);

            if (reference is null)
            {
                reference = new MatchReference
                {
                    MatchId = matchId,
                    QueueType = queueType
                };

                _db.MatchReferences.Add(reference);
                existingRefs.Add(reference);
            }

            if (!summonerExistingIds.Contains(matchId))
            {
                summoner.MatchReferences.Add(reference);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<MatchDto>> GetSummonerMatchesAsync(
        string puuid,
        QueueType queueType = QueueType.DRAFT_PICK,
        CancellationToken ct = default)
    {
        List<MatchReference> matchReferences = await GetMatchReferencesAsync(puuid, queueType, ct);

        Dictionary<string, Summoner> summonerCache = [];

        Dictionary<int, Champion> champions =
            await _db.Champions.ToDictionaryAsync(c => c.Key, ct);

        Dictionary<int, SummonerSpell> summonerSpells =
            await _db.SummonerSpells.ToDictionaryAsync(s => s.Key, ct);

        List<Match> matches = [];

        foreach (MatchReference reference in matchReferences)
        {
            Match match = reference.Match ??
                await CreateMatchAsync(
                    reference,
                    summonerCache,
                    champions,
                    summonerSpells,
                    ct);

            matches.Add(match);
        }

        await _db.SaveChangesAsync(ct);

        return [.. matches.Select(m => MatchToMatchDto(m, puuid))];
    }

    private async Task<List<MatchReference>> GetMatchReferencesAsync(
        string puuid,
        QueueType queueType,
        CancellationToken ct)
    {
        return await _db.MatchReferences
            .Include(m => m.Match)
                .ThenInclude(m => m!.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.Champion)
            .Include(m => m.Match)
                .ThenInclude(m => m!.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.Summoner)
            .Include(m => m.Match)
                .ThenInclude(m => m!.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.SummonerSpells)
            .Where(m =>
                m.Summoners.Any(s => s.Puuid == puuid) &&
                m.QueueType == queueType)
            .OrderByDescending(m => m.MatchId)
            .Take(10)
            .ToListAsync(ct);
    }

    private async Task<Match> CreateMatchAsync(
        MatchReference reference,
        Dictionary<string, Summoner> summonerCache,
        Dictionary<int, Champion> champions,
        Dictionary<int, SummonerSpell> spells,
        CancellationToken ct)
    {
        MatchResponseDto dto =
            await _riotApiService.GetMatchDetailAsync(reference.MatchId, ct);

        InfoDto info = dto.Info;

        List<Team> teams = [.. info.Teams
            .Select(t => new Team
            {
                TeamId = t.TeamId,
                Win = t.Win
            })];

        _db.Teams.AddRange(teams);

        List<Participant> participants = [];

        foreach (ParticipantDto participantDto in info.Participants)
        {
            participants.Add(await CreateParticipantAsync(
                participantDto,
                teams,
                champions,
                spells,
                summonerCache,
                ct));
        }

        _db.Participants.AddRange(participants);

        Match match = new()
        {
            EndOfGameResult = info.EndOfGameResult,
            GameDuration = info.GameDuration,
            GameEndTimestamp = info.GameEndTimestamp,
            QueueType = QueueTypeHelper.QueueIdToQueueType(info.QueueId),
            MatchReference = reference,
            Teams = teams
        };

        _db.Matches.Add(match);

        return match;
    }

    private async Task<Participant> CreateParticipantAsync(
        ParticipantDto dto,
        List<Team> teams,
        Dictionary<int, Champion> champions,
        Dictionary<int, SummonerSpell> spells,
        Dictionary<string, Summoner> cache,
        CancellationToken ct)
    {
        if (!champions.TryGetValue(dto.ChampionId, out Champion? champion))
            throw new NotFoundException(nameof(Champion), nameof(Champion.Key), dto.ChampionId);

        if (!spells.TryGetValue(dto.Summoner1Id, out SummonerSpell? spell1))
            throw new NotFoundException(nameof(SummonerSpell), nameof(SummonerSpell.Key), dto.Summoner1Id);

        if (!spells.TryGetValue(dto.Summoner2Id, out SummonerSpell? spell2))
            throw new NotFoundException(nameof(SummonerSpell), nameof(SummonerSpell.Key), dto.Summoner2Id);

        Summoner summoner = await GetOrCreateSummonerAsync(dto, cache, ct);

        return new Participant
        {
            ParticipantId = dto.ParticipantId,
            Assists = dto.Assists,
            ChampionLevel = dto.ChampLevel,
            Deaths = dto.Deaths,
            Gold = dto.GoldEarned,
            Items =
            [
                dto.Item0,
                dto.Item1,
                dto.Item2,
                dto.Item3,
                dto.Item4,
                dto.Item5,
                dto.Item6
            ],
            Kills = dto.Kills,
            Lane = dto.TeamPosition,
            PrimaryRune = dto.Perks.Styles[0].Selections[0].Perk,
            SecondaryTree = dto.Perks.Styles[1].Style,
            DamageToChampions = dto.TotalDamageDealtToChampions,
            Team = teams.First(t => t.TeamId == dto.TeamId),
            Champion = champion,
            Summoner = summoner,
            SummonerSpells = [spell1, spell2]
        };
    }

    private async Task<Summoner> GetOrCreateSummonerAsync(
        ParticipantDto dto,
        Dictionary<string, Summoner> cache,
        CancellationToken ct)
    {
        if (cache.TryGetValue(dto.Puuid, out Summoner? summoner))
            return summoner;

        summoner = await _db.Summoners
            .FirstOrDefaultAsync(s => s.Puuid == dto.Puuid, ct);

        if (summoner is null)
        {
            summoner = new Summoner
            {
                Puuid = dto.Puuid,
                Username = dto.RiotIdGameName,
                Tag = dto.RiotIdTagLine,
                Level = dto.SummonerLevel,
                ProfileIconId = dto.ProfileIcon
            };

            _db.Summoners.Add(summoner);
        }

        cache[dto.Puuid] = summoner;

        return summoner;
    }

    private static MatchDto MatchToMatchDto(Match match, string puuid)
    {
        Participant participant = match.Teams.SelectMany(t => t.Participants).First(p => p.Summoner.Puuid == puuid);

        return new MatchDto
        {
            EndOfGameResult = match.EndOfGameResult,
            GameDuration = match.GameDuration,
            GameEndTimestamp = match.GameEndTimestamp,
            QueueType = match.QueueType,
            Win = participant.Team.Win,
            Assists = participant.Assists,
            ChampionLevel = participant.ChampionLevel,
            ChampionName = participant.Champion.Name,
            Deaths = participant.Deaths,
            Gold = participant.Gold,
            Items = participant.Items,
            Kills = participant.Kills,
            Lane = participant.Lane,
            PrimaryRune = participant.PrimaryRune,
            SecondaryTree = participant.SecondaryTree,
            DamageToChampions = participant.DamageToChampions,
            Participants = [.. match.Teams
                .SelectMany(t => t.Participants)
                .Select(ParticipantToParticipantBriefDto)],
            SummonerSpells = [.. participant.SummonerSpells.Select(SummonerSpellToSummonerSpellDto)]
        };
    }

    private static ParticipantBriefDto ParticipantToParticipantBriefDto(Participant participant)
    {
        return new ParticipantBriefDto
        {
            ChampionName = participant.Champion.Name,
            SummonerName = participant.Summoner.Username,
            Lane = participant.Lane,
            TeamId = participant.Team.TeamId
        };
    }

    private static SummonerSpellDto SummonerSpellToSummonerSpellDto(SummonerSpell summonerSpell)
    {
        return new SummonerSpellDto
        {
            Key = summonerSpell.Key,
            Name = summonerSpell.Name
        };
    }
}