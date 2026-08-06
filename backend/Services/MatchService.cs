using backend.Data;
using backend.Exceptions;
using backend.Helpers;
using backend.Interfaces;
using backend.Mappers;
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
            ?? throw new NotFoundException("Summoner not found", nameof(Summoner), nameof(Summoner.Puuid), puuid);

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

    public async Task<PagedResult<MatchDto>> GetSummonerMatchesAsync(
        string puuid,
        int page = 1,
        int pageSize = 10,
        QueueType queueType = QueueType.DRAFT_PICK,
        CancellationToken ct = default)
    {
        var query = _db.MatchReferences
            .Where(m =>
                m.Summoners.Any(s => s.Puuid == puuid) &&
                m.QueueType == queueType);

        int totalItems = await query.CountAsync(ct);

        List<MatchReference> matchReferences = await query
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
            .Include(m => m.Match)
                .ThenInclude(m => m!.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.Items)
            .Include(m => m.Match)
                .ThenInclude(m => m!.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.PrimaryRune)
            .Include(m => m.Match)
                .ThenInclude(m => m!.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.SecondaryTree)
            .OrderByDescending(m => m.Match!.GameEndTimestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        Dictionary<string, Summoner> summonerCache = [];

        Dictionary<int, Champion> champions =
            await _db.Champions.ToDictionaryAsync(c => c.Key, ct);

        Dictionary<int, SummonerSpell> summonerSpells =
            await _db.SummonerSpells.ToDictionaryAsync(s => s.Key, ct);

        Dictionary<int, Item> items =
            await _db.Items.ToDictionaryAsync(i => i.Key, ct);

        Dictionary<int, Rune> runes =
            await _db.Runes.ToDictionaryAsync(r => r.RiotId, ct);

        List<Match> matches = [];

        foreach (MatchReference reference in matchReferences)
        {
            Match match = reference.Match ??
                await CreateMatchAsync(
                    reference,
                    summonerCache,
                    champions,
                    summonerSpells,
                    items,
                    runes,
                    ct);

            matches.Add(match);
        }

        await _db.SaveChangesAsync(ct);

        return new PagedResult<MatchDto>
        {
            Items = [.. matches.Select(m => MatchToMatchDto(m, puuid))],
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private async Task<Match> CreateMatchAsync(
        MatchReference reference,
        Dictionary<string, Summoner> summonerCache,
        Dictionary<int, Champion> champions,
        Dictionary<int, SummonerSpell> spells,
        Dictionary<int, Item> items,
        Dictionary<int, Rune> runes,
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
                items,
                runes,
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
        Dictionary<int, Item> items,
        Dictionary<int, Rune> runes,
        Dictionary<string, Summoner> cache,
        CancellationToken ct)
    {
        if (!champions.TryGetValue(dto.ChampionId, out Champion? champion))
            throw new NotFoundException("Could not create match participants", nameof(Champion), nameof(Champion.Key), dto.ChampionId);

        if (!spells.TryGetValue(dto.Summoner1Id, out SummonerSpell? spell1))
            throw new NotFoundException("Could not create match participants", nameof(SummonerSpell), nameof(SummonerSpell.Key), dto.Summoner1Id);

        if (!spells.TryGetValue(dto.Summoner2Id, out SummonerSpell? spell2))
            throw new NotFoundException("Could not create match participants", nameof(SummonerSpell), nameof(SummonerSpell.Key), dto.Summoner2Id);

        if (!runes.TryGetValue(dto.Perks.Styles[0].Selections[0].Perk, out Rune? primaryRune))
            throw new NotFoundException("Could not create match participants", nameof(Rune), nameof(Rune.RiotId), dto.Perks.Styles[0].Selections[0].Perk);

        if (!runes.TryGetValue(dto.Perks.Styles[1].Style, out Rune? secondaryTree))
            throw new NotFoundException("Could not create match participants", nameof(Rune), nameof(Rune.RiotId), dto.Perks.Styles[1].Style);

        Summoner summoner = await GetOrCreateSummonerAsync(dto, cache, ct);

        int[] itemIds =
        [
            dto.Item0,
            dto.Item1,
            dto.Item2,
            dto.Item3,
            dto.Item4,
            dto.Item5,
            dto.Item6
        ];

        List<Item> participantItems = [];

        foreach (int itemId in itemIds)
        {
            if (itemId == 0)
            {
                continue;
            }

            if (!items.TryGetValue(itemId, out Item? item))
            {
                throw new NotFoundException("Could not create match participants", nameof(Item), nameof(Item.Key), itemId);
            }

            participantItems.Add(item);
        }

        return new Participant
        {
            ParticipantId = dto.ParticipantId,
            Assists = dto.Assists,
            ChampionLevel = dto.ChampLevel,
            Deaths = dto.Deaths,
            Gold = dto.GoldEarned,
            Items = participantItems,
            Kills = dto.Kills,
            Lane = dto.TeamPosition,
            Minions = dto.TotalMinionsKilled + dto.NeutralMinionsKilled,
            DamageToChampions = dto.TotalDamageDealtToChampions,
            Team = teams.First(t => t.TeamId == dto.TeamId),
            PrimaryRune = primaryRune,
            SecondaryTree = secondaryTree,
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
            MatchId = match.MatchReference.MatchId,
            EndOfGameResult = match.EndOfGameResult,
            GameDuration = match.GameDuration,
            GameEndTimestamp = match.GameEndTimestamp,
            QueueType = match.QueueType,
            Win = participant.Team.Win,
            Participants = [.. match.Teams
                .SelectMany(t => t.Participants)
                .Select(ParticipantToParticipantDetailDto)],
        };
    }

    private static ParticipantDetailDto ParticipantToParticipantDetailDto(Participant participant)
    {
        return new ParticipantDetailDto
        {
            Puuid = participant.Summoner.Puuid,
            SummonerName = participant.Summoner.Username,
            SummonerTag = participant.Summoner.Tag,
            Assists = participant.Assists,
            ChampionLevel = participant.ChampionLevel,
            Deaths = participant.Deaths,
            Gold = participant.Gold,
            Kills = participant.Kills,
            Lane = participant.Lane,
            Minions = participant.Minions,
            DamageToChampions = participant.DamageToChampions,
            TeamId = participant.Team.TeamId,
            PrimaryRune = RuneMapper.RuneToRuneDto(participant.PrimaryRune),
            SecondaryTree = RuneMapper.RuneToRuneDto(participant.SecondaryTree),
            Champion = ChampionMapper.ChampionToChampionDto(participant.Champion),
            Items = [.. participant.Items.Select(ItemToItemDtoDto)],
            SummonerSpells = [.. participant.SummonerSpells.Select(SummonerSpellMapper.SummonerSpellToSummonerSpellDto)]
        };
    }

    private static ItemDto ItemToItemDtoDto(Item item)
    {
        return new ItemDto
        {
            Key = item.Key,
            Name = item.Name,
            Description = item.Description,
            BuyPrice = item.BuyPrice,
            SellPrice = item.SellPrice,
            Stats = item.Stats
        };
    }
}