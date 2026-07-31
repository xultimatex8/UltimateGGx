using backend.Data;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Riot;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class TimelineService : ITimelineService
{
    private readonly AppDbContext _db;
    private readonly IRiotApiService _riotApiService;

    public TimelineService(AppDbContext db, IRiotApiService riotApiService)
    {
        _db = db;
        _riotApiService = riotApiService;
    }

    public async Task GetOrFetchTimelineAsync(string matchId, CancellationToken ct = default)
    {
        MatchReference reference = await _db.MatchReferences
            .Include(mr => mr.Match!)
                .ThenInclude(m => m.Events)
            .FirstOrDefaultAsync(mr => mr.MatchId == matchId, ct)
            ?? throw new NotFoundException(nameof(MatchReference), nameof(MatchReference.MatchId), matchId);

        if (reference.Match is null)
        {
            throw new NotFoundException(nameof(MatchReference.Match), nameof(MatchReference.MatchId), matchId);
        }

        if (reference.Match.Events.Count == 0)
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
            ?? throw new NotFoundException(nameof(MatchReference), nameof(MatchReference.MatchId), matchId);

        Match match = reference.Match
            ?? throw new NotFoundException(nameof(MatchReference.Match), nameof(MatchReference.MatchId), matchId);

        TimelineResponseDto timeline = await _riotApiService.GetMatchTimelineAsync(matchId, ct);

        Dictionary<int, Participant> participants = match.Teams
            .SelectMany(t => t.Participants)
            .ToDictionary(p => p.ParticipantId);

        List<Event> events = [];
        List<ParticipantFrame> frames = [];

        foreach (FramesTimeLineDto frame in timeline.Info.Frames)
        {
            foreach (EventsTimeLineDto eventDto in frame.Events)
            {
                Event? mappedEvent = MapEvent(eventDto, match, participants);

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

    private static Event? MapEvent(
        EventsTimeLineDto dto,
        Match match,
        Dictionary<int, Participant> participants)
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
            ItemId = dto.ItemId != 0 ? dto.ItemId : null,
            MonsterType = ParseEnumOrNull<MonsterType>(dto.MonsterType),
            MonsterSubType = ParseEnumOrNull<MonsterSubType>(dto.MonsterSubType),
            BuildingType = ParseEnumOrNull<BuildingType>(dto.BuildingType),
            LaneType = ParseEnumOrNull<LaneType>(dto.LaneType),
            TowerType = ParseEnumOrNull<TowerType>(dto.TowerType)
        };

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

        foreach (int assistingId in dto.AssistingParticipantIds)
        {
            if (participants.TryGetValue(assistingId, out Participant? assisting))
            {
                newEvent.AssistingParticipants.Add(assisting);
            }
        }

        return newEvent;
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