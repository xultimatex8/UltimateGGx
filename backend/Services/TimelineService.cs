using backend.Data;
using backend.Exceptions;
using backend.Interfaces;
using backend.Mappers;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Enums;
using backend.Models.Riot;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class TimelineService : ITimelineService
{
    private readonly AppDbContext _db;
    private readonly IRiotApiService _riotApiService;
    private readonly IMatchService _matchService;

    public TimelineService(AppDbContext db, IRiotApiService riotApiService, IMatchService matchService)
    {
        _db = db;
        _riotApiService = riotApiService;
        _matchService = matchService;
    }

    public async Task CheckOrFetchTimelineAsync(string matchId, CancellationToken ct = default)
    {
        await _matchService.GetOrCreateMatchAsync(matchId, ct);

        MatchReference reference = await _db.MatchReferences
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
            .FirstAsync(mr => mr.MatchId == matchId, ct);

        if (reference.Match!.Events.Count == 0)
        {
            await SyncTimelineAsync(matchId, ct);
        }
    }

    public async Task SyncTimelineAsync(string matchId, CancellationToken ct = default)
    {
        MatchReference reference = await _db.MatchReferences
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.Frames)
            .FirstOrDefaultAsync(mr => mr.MatchId == matchId, ct)
            ?? throw new NotFoundException("Match not found", nameof(MatchReference), nameof(MatchReference.MatchId), matchId);

        Match match = reference.Match
            ?? throw new NotFoundException("Match not found", nameof(MatchReference.Match), nameof(MatchReference.MatchId), matchId);

        TimelineResponseDto timeline = await _riotApiService.GetMatchTimelineAsync(matchId, ct);

        Dictionary<int, Participant> participants = match.Teams
            .SelectMany(t => t.Participants)
            .ToDictionary(p => p.ParticipantId);

        Dictionary<int, Team> teams = match.Teams
            .ToDictionary(t => t.TeamId);

        Dictionary<int, Item> items = await _db.Items
            .ToDictionaryAsync(i => i.Key, ct);

        List<Event> events = [];
        List<ParticipantFrame> frames = [];

        foreach (FramesTimeLineDto frame in timeline.Info.Frames)
        {
            foreach (EventsTimeLineDto eventDto in frame.Events)
            {
                Event? mappedEvent = MapEvent(eventDto, match, items, participants, teams);

                if (mappedEvent is not null)
                {
                    events.Add(mappedEvent);
                }
            }

            foreach ((string participantKey, ParticipantFrameDto frameDto) in frame.ParticipantFrames)
            {
                if (!int.TryParse(participantKey, out int participantId) ||
                    !participants.TryGetValue(participantId, out Participant? participant))
                {
                    continue;
                }

                frames.Add(new ParticipantFrame
                {
                    CurrentGold = frameDto.CurrentGold,
                    Minions = frameDto.MinionsKilled + frameDto.JungleMinionsKilled,
                    Level = frameDto.Level,
                    Timestamp = frame.Timestamp,
                    TotalGold = frameDto.TotalGold,
                    PositionX = frameDto.Position.X,
                    PositionY = frameDto.Position.Y,
                    Participant = participant
                });
            }
        }

        match.Events = events;

        _db.ParticipantFrames.AddRange(frames);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<TimelineDto> GetTimelineAsync(string matchId, CancellationToken ct = default)
    {
        await CheckOrFetchTimelineAsync(matchId, ct);

        MatchReference reference = await _db.MatchReferences
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Item)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Participant)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Killer)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Victim)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.AssistingParticipants)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Team)
            .FirstAsync(mr => mr.MatchId == matchId, ct);

        Match match = reference.Match
            ?? throw new NotFoundException("Match not found", nameof(MatchReference.Match), nameof(MatchReference.MatchId), matchId);

        return MatchToTimelineDto(match);
    }

    public async Task<ScoreboardDto> GetScoreboardAsync(string matchId, long timestamp, CancellationToken ct = default)
    {
        await CheckOrFetchTimelineAsync(matchId, ct);

        MatchReference reference = await _db.MatchReferences
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.Champion)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.Summoner)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.SummonerSpells)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.PrimaryRune)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.SecondaryTree)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Teams)
                    .ThenInclude(t => t.Participants)
                        .ThenInclude(p => p.Frames)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Item)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Participant)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Killer)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.Victim)
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
                    .ThenInclude(e => e.AssistingParticipants)
            .FirstAsync(mr => mr.MatchId == matchId, ct);

        Match match = reference.Match
            ?? throw new NotFoundException("Match not found", nameof(MatchReference.Match), nameof(MatchReference.MatchId), matchId);

        return MatchToScoreboardDto(match, timestamp);
    }

    private static Event? MapEvent(
        EventsTimeLineDto dto,
        Match match,
        Dictionary<int, Item> items,
        Dictionary<int, Participant> participants,
        Dictionary<int, Team> teams)
    {
        if (!Enum.TryParse(dto.Type, out EventType type))
        {
            return null;
        }

        Event newEvent = new()
        {
            Timestamp = dto.Timestamp,
            Type = type,
            Match = match,
            Bounty = dto.Bounty != 0 ? dto.Bounty : null,
            ShutdownBounty = dto.ShutdownBounty != 0 ? dto.ShutdownBounty : null,
            MonsterType = ParseEnumOrNull<MonsterType>(dto.MonsterType),
            MonsterSubType = ParseEnumOrNull<MonsterSubType>(dto.MonsterSubType),
            BuildingType = ParseEnumOrNull<BuildingType>(dto.BuildingType),
            LaneType = ParseEnumOrNull<LaneType>(dto.LaneType),
            TowerType = ParseEnumOrNull<TowerType>(dto.TowerType),
        };

        if (items.TryGetValue(dto.ItemId, out Item? item))
        {
            newEvent.Item = item;
        }

        if (items.TryGetValue(dto.BeforeId, out Item? beforeItem))
        {
            newEvent.BeforeItem = beforeItem;
        }

        if (items.TryGetValue(dto.AfterId, out Item? afterItem))
        {
            newEvent.AfterItem = afterItem;
        }

        if (participants.TryGetValue(dto.ParticipantId, out Participant? participant))
        {
            newEvent.Participant = participant;
        }

        if (participants.TryGetValue(dto.KillerId, out Participant? killer))
        {
            newEvent.Killer = killer;
        }

        if (participants.TryGetValue(dto.VictimId, out Participant? victim))
        {
            newEvent.Victim = victim;
        }

        if (teams.TryGetValue(dto.TeamId, out Team? team))
        {
            newEvent.Team = team;
        }

        if (teams.TryGetValue(dto.WinningTeam, out Team? winningTeam))
        {
            newEvent.Team = winningTeam;
        }

        foreach (int assistingId in dto.AssistingParticipantIds)
        {
            if (participants.TryGetValue(assistingId, out Participant? assisting))
            {
                newEvent.AssistingParticipants.Add(assisting);
            }
        }

        return newEvent;
    }

    private static TimelineDto MatchToTimelineDto(Match match)
    {
        List<Event> visibleEvents = RemoveUndoneAndDestroyedItemEvents(match.Events);

        return new TimelineDto
        {
            Events = [.. visibleEvents
                .OrderBy(e => e.Timestamp)
                .Select(e => EventToEventDto(e, match))]
        };
    }

    private static List<Event> RemoveUndoneAndDestroyedItemEvents(IEnumerable<Event> events)
    {
        List<Event> ordered = [.. events.OrderBy(e => e.Timestamp)];
        HashSet<Event> toRemove = [];

        foreach (Event undo in ordered.Where(e => e.Type == EventType.ITEM_UNDO))
        {
            int? targetItemKey = undo.BeforeItem?.Key ?? undo.AfterItem?.Key;

            toRemove.Add(undo);

            if (targetItemKey is null)
            {
                continue;
            }

            Event? cancelled = ordered
                .Where(e =>
                    e.Timestamp <= undo.Timestamp &&
                    e != undo &&
                    !toRemove.Contains(e) &&
                    e.Participant?.Id == undo.Participant?.Id &&
                    e.Item?.Key == targetItemKey &&
                    (e.Type == EventType.ITEM_PURCHASED ||
                    e.Type == EventType.ITEM_SOLD ||
                    e.Type == EventType.ITEM_DESTROYED))
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault();

            if (cancelled is not null)
            {
                toRemove.Add(cancelled);
            }
        }

        foreach (Event destroyed in ordered.Where(e => e.Type == EventType.ITEM_DESTROYED))
        {
            toRemove.Add(destroyed);
        }

        foreach (Event destroyed in ordered.Where(e => e.Type == EventType.ITEM_PURCHASED && e.Participant == null && e.Killer == null))
        {
            toRemove.Add(destroyed);
        }

        return [.. ordered.Where(e => !toRemove.Contains(e))];
    }

    private static EventDto EventToEventDto(Event evt, Match match)
    {
        return new EventDto
        {
            Timestamp = evt.Timestamp,
            Bounty = evt.Bounty,
            ShutdownBounty = evt.ShutdownBounty,
            MonsterType = evt.MonsterType,
            MonsterSubType = evt.MonsterSubType,
            BuildingType = evt.BuildingType,
            LaneType = evt.LaneType,
            TowerType = evt.TowerType,
            Type = evt.Type,
            MainParticipantId = evt.Killer?.ParticipantId ?? evt.Participant?.ParticipantId,
            VictimParticipantId = evt.Victim?.ParticipantId,
            AssistingParticipants = [.. evt.AssistingParticipants.Select(p => p.ParticipantId)],
            Item = evt.Item is not null ? ItemToItemDto(evt.Item) : null,
            TeamId = evt.Team is not null ? evt.Team.TeamId : null
        };
    }

    private static ScoreboardDto MatchToScoreboardDto(Match match, long timestamp)
    {
        List<Event> pastEvents = [.. match.Events
            .Where(e => e.Timestamp <= timestamp)
            .OrderBy(e => e.Timestamp)];

        return new ScoreboardDto
        {
            Timestamp = timestamp,
            Teams = [.. match.Teams.Select(t => TeamToScoreboardTeamDto(t, pastEvents, timestamp))]
        };
    }

    private static ScoreboardTeamDto TeamToScoreboardTeamDto(Team team, List<Event> pastEvents, long timestamp)
    {
        return new ScoreboardTeamDto
        {
            TeamId = team.TeamId,
            Participants = [.. team.Participants.Select(p => ParticipantToScoreboardParticipantDto(p, pastEvents, timestamp))]
        };
    }

    private static ScoreboardParticipantDto ParticipantToScoreboardParticipantDto(
        Participant participant,
        List<Event> pastEvents,
        long timestamp)
    {
        ParticipantFrame? frame = participant.Frames
            .Where(f => f.Timestamp <= timestamp)
            .OrderByDescending(f => f.Timestamp)
            .FirstOrDefault();

        int kills = pastEvents.Count(e => e.Killer?.Id == participant.Id);
        int deaths = pastEvents.Count(e => e.Victim?.Id == participant.Id);
        int assists = pastEvents.Count(e => e.AssistingParticipants.Any(a => a.Id == participant.Id));

        List<Item> currentItems = BuildCurrentItems(pastEvents, participant);

        return new ScoreboardParticipantDto
        {
            ParticipantId = participant.ParticipantId,
            SummonerName = participant.Summoner.Username,
            Assists = assists,
            ChampionLevel = frame?.Level ?? 1,
            Deaths = deaths,
            CurrentGold = frame?.CurrentGold ?? 0,
            TotalGold = frame?.TotalGold ?? 0,
            Kills = kills,
            Lane = participant.Lane,
            Minions = frame?.Minions ?? 0,
            PositionX = frame?.PositionX ?? 0,
            PositionY = frame?.PositionY ?? 0,
            PrimaryRune = RuneMapper.RuneToRuneDto(participant.PrimaryRune),
            SecondaryTree = RuneMapper.RuneToRuneDto(participant.SecondaryTree),
            Champion = ChampionMapper.ChampionToChampionDto(participant.Champion),
            SummonerSpells = [.. participant.SummonerSpells.Select(SummonerSpellMapper.SummonerSpellToSummonerSpellDto)],
            Items = [.. currentItems.Select(ItemToItemDto)]
        };
    }

    private static List<Item> BuildCurrentItems(List<Event> pastEvents, Participant participant)
    {
        List<Item> inventory = [];

        foreach (Event evt in pastEvents)
        {
            if (evt.Participant?.Id != participant.Id)
            {
                continue;
            }

            switch (evt.Type)
            {
                case EventType.ITEM_PURCHASED when evt.Item is not null:
                    inventory.Add(evt.Item);
                    break;

                case EventType.ITEM_SOLD when evt.Item is not null:
                case EventType.ITEM_DESTROYED when evt.Item is not null:
                    inventory.Remove(evt.Item);
                    break;

                case EventType.ITEM_UNDO:
                    if (evt.BeforeItem is not null)
                    {
                        inventory.Remove(evt.BeforeItem);
                    }

                    if (evt.AfterItem is not null)
                    {
                        inventory.Add(evt.AfterItem);
                    }
                    break;
            }
        }

        return inventory;
    }

    private static ItemDto ItemToItemDto(Item item)
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

    private static TEnum? ParseEnumOrNull<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return Enum.TryParse(value, out TEnum result) ? result : null;
    }
}