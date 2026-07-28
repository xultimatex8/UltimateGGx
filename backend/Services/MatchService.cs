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

    public async Task<List<MatchDto>> GetSummonerMatchesAsync(string puuid, QueueType queueType = QueueType.DRAFT_PICK, CancellationToken ct = default)
    {
        List<MatchReference> matchReferences = await _db.MatchReferences
            .Include(m => m.Match)
                .ThenInclude(match => match!.Teams)
                    .ThenInclude(team => team.Participants)
                        .ThenInclude(p => p.Champion)
            .Include(m => m.Match)
                .ThenInclude(match => match!.Teams)
                    .ThenInclude(team => team.Participants)
                        .ThenInclude(p => p.Summoner)
            .Include(m => m.Match)
                .ThenInclude(match => match!.Teams)
                    .ThenInclude(team => team.Participants)
                        .ThenInclude(p => p.SummonerSpells)
            .Where(m => m.Summoners.Any(s => s.Puuid == puuid) && m.QueueType == queueType)
            .OrderByDescending(m => m.MatchId)
            .Take(10)
            .ToListAsync(ct);

        List<Match> matches = [];
        Dictionary<string, Summoner> summonersInThisRun = [];

        foreach (MatchReference matchReference in matchReferences)
        {
            Match? match = matchReference.Match;

            if (match == null)
            {  
                MatchResponseDto matchResponseDto = await _riotApiService.GetMatchDetailAsync(matchReference.MatchId, ct);
                InfoDto info = matchResponseDto.Info;

                List<Team> teams = [];
                List<Participant> participants = [];

                foreach (TeamDto teamDto in info.Teams)
                {
                    Team team = new()
                    {
                        Win = teamDto.Win,
                        TeamId = teamDto.TeamId
                    };

                    teams.Add(team);
                }

                _db.Teams.AddRange(teams);

                foreach (ParticipantDto participantDto in info.Participants)
                {
                    Champion champion = await _db.Champions.FirstOrDefaultAsync(c => c.Key == participantDto.ChampionId) ?? 
                        throw new NotFoundException(nameof(Champion), nameof(Champion.Key), participantDto.ChampionId);
                        
                    List<SummonerSpell> summonerSpells = await _db.SummonerSpells
                        .Where(sp => sp.Key == participantDto.Summoner1Id ||
                                    sp.Key == participantDto.Summoner2Id)
                        .ToListAsync(ct);

                    string summonerPuuid = participantDto.Puuid;

                    if (!summonersInThisRun.TryGetValue(summonerPuuid, out Summoner? summoner))
                    {
                        summoner = await _db.Summoners.FirstOrDefaultAsync(s => s.Puuid == summonerPuuid, ct);

                        if (summoner is null)
                        {
                            summoner = new Summoner
                            {
                                Puuid = summonerPuuid,
                                Username = participantDto.RiotIdGameName,
                                Tag = participantDto.RiotIdTagLine,
                                Level = participantDto.SummonerLevel,
                                ProfileIconId = participantDto.ProfileIcon,
                            };
                            _db.Summoners.Add(summoner);
                        }

                        summonersInThisRun[summonerPuuid] = summoner;
                    }

                    Participant participant = new()
                    {
                        ParticipantId = participantDto.ParticipantId,
                        Assists = participantDto.Assists,
                        ChampionLevel = participantDto.ChampLevel,
                        Deaths = participantDto.Deaths,
                        Gold = participantDto.GoldEarned,
                        Items = [participantDto.Item0, participantDto.Item1, 
                            participantDto.Item2, participantDto.Item3, 
                            participantDto.Item4, participantDto.Item5, 
                            participantDto.Item6],
                        Kills = participantDto.Kills,
                        Lane = participantDto.TeamPosition,
                        PrimaryRune = participantDto.Perks.Styles[0].Selections[0].Perk,
                        SecondaryTree = participantDto.Perks.Styles[1].Style,
                        DamageToChampions = participantDto.TotalDamageDealtToChampions,
                        Team = teams.First(t => t.TeamId == participantDto.TeamId),
                        Summoner = summoner,
                        Champion = champion,
                        SummonerSpells = summonerSpells
                    };

                    participants.Add(participant);
                }

                _db.Participants.AddRange(participants);

                match = new Match
                {
                    EndOfGameResult = info.EndOfGameResult,
                    GameDuration = info.GameDuration,
                    GameEndTimestamp = info.GameEndTimestamp,
                    QueueType = QueueTypeHelper.QueueIdToQueueType(info.QueueId),
                    MatchReference = matchReference,
                    Teams = teams
                };

                _db.Matches.Add(match);
            }

            matches.Add(match);
        }

        await _db.SaveChangesAsync(ct);

        return [.. matches.Select(m => MatchToMatchDto(m, puuid))];
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