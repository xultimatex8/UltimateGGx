import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DataDragon } from '../../shared/data-dragon/data-dragon';
import { FormatDurationUtil } from '../../shared/utils/format-duration.util';
import { MatchScoreboard } from '../match-scoreboard/match-scoreboard';
import { Timeline } from '../timeline';
import { TimelineDto, ScoreboardDto, EventDto, ScoreboardParticipantDto } from '../timeline.model';
import { EventType } from '../../shared/enums/event-type';
import { TowerType, BuildingType } from '../../shared/enums/building-type';
import { LaneType } from '../../shared/enums/lane-type';
import { MonsterType, MonsterSubType } from '../../shared/enums/monster-type';
import { DataDragonUrlUtil } from '../../shared/utils/data-dragon-url.utils';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-match-timeline',
  imports: [MatchScoreboard],
  templateUrl: './match-timeline.html',
})
export class MatchTimeline {
  private route = inject(ActivatedRoute);
  private timelineService = inject(Timeline);
  private dataDragon = inject(DataDragon);

  protected readonly ddragonVersion = this.dataDragon.version();
  protected readonly FormatDurationUtil = FormatDurationUtil;
  protected readonly DataDragonUrlUtil = DataDragonUrlUtil;

  protected readonly MAP_SIZE = 14820;

  matchId = signal<string | null>(null);
  timeline = signal<TimelineDto | null>(null);
  scoreboard = signal<ScoreboardDto | null>(null);
  selectedTimestamp = signal<number | null>(null);
  selectedEvent = signal<EventDto | null>(null);

  loadingTimeline = signal(true);
  loadingScoreboard = signal(false);
  initialLoadComplete = signal(false);
  error = signal<string | null>(null);

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('matchId') || params.get('id');
      this.matchId.set(id);

      if (id) {
        this.fetchTimeline(id);
      }
    });

    effect(() => {
      if (this.scoreboard() && !this.initialLoadComplete()) {
        this.initialLoadComplete.set(true);
      }
    });
  }

  protected getEventTitle(event: EventDto): string {
    if (event.monsterType) {
      return `${event.monsterSubType ? event.monsterType + ':' : event.monsterType} ${event.monsterSubType || ''}`
        .trim()
        .toLowerCase()
        .replace(/_/g, ' ')
        .replace(/\b\w/g, (char) => char.toUpperCase());
    }
    if (event.buildingType) {
      return `${event.buildingType} (${event.laneType || ''})`
        .toLowerCase()
        .replace(/_/g, ' ')
        .replace(/\b\w/g, (char) => char.toUpperCase());
    }
    if (event.type === EventType.TURRET_PLATE_DESTROYED) {
      return `${this.formatLane(event.laneType)} turret plate`
        .trim()
        .toLowerCase()
        .replace(/\b\w/g, (char) => char.toUpperCase());
    }
    if (event.item) {
      return 'Item Purchased';
    }
    
    const cleanType = String(event.type || 'Event')
      .toLowerCase()
      .replace(/_/g, ' ');

    return cleanType.replace(/\b\w/g, (char) => char.toUpperCase());
  }

  public selectEvent(event: EventDto) {
    this.selectedEvent.set(event);
    this.selectedTimestamp.set(event.timestamp);

    const currentId = this.matchId();
    if (currentId) {
      this.fetchScoreboard(currentId, event.timestamp);
    }
  }

  private fetchTimeline(matchId: string) {
    this.loadingTimeline.set(true);
    this.error.set(null);

    this.timelineService.getMatchTimeline(matchId).subscribe({
      next: (timeline) => {
        this.timeline.set(timeline);
        this.loadingTimeline.set(false);
        this.fetchScoreboard(matchId, 0);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(err.error?.error ?? 'An unexpected error occurred.');
        this.loadingTimeline.set(false);
      },
    });
  }

  private fetchScoreboard(matchId: string, timestamp?: number) {
    this.loadingScoreboard.set(true);
    this.error.set(null);

    this.timelineService.getMatchScoreboard(matchId, timestamp).subscribe({
      next: (scoreboard) => {
        this.scoreboard.set(scoreboard);
        this.loadingScoreboard.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(err.error?.error ?? 'An unexpected error occurred.');
        this.loadingScoreboard.set(false);
      },
    });
  }

  getParticipantByParticipantId(participantId?: number): ScoreboardParticipantDto | undefined {
    return this.scoreboard()
      ?.teams
      .flatMap(team => team.participants)
      .find(participant => participant.participantId === participantId);
  }

  formatLane(lane?: LaneType): string {
    switch (lane) {
      case LaneType.TOP_LANE:
        return 'Top';
      case LaneType.MID_LANE:
        return 'Mid';
      case LaneType.BOT_LANE:
        return 'Bot';
      default:
        return '';
    }
  }

  formatTower(tower?: TowerType): string {
    switch (tower) {
      case TowerType.OUTER_TURRET:
        return 'Outer Turret';
      case TowerType.INNER_TURRET:
        return 'Inner Turret';
      case TowerType.BASE_TURRET:
        return 'Base Turret';
      case TowerType.NEXUS_TURRET:
        return 'Nexus Turret';
      default:
        return 'Turret';
    }
  }

  formatMonster(event: EventDto): string {
    if (event.monsterType !== MonsterType.DRAGON) {
      if (!event.monsterType) return 'Monster';

      return event.monsterType
        .trim()
        .toLowerCase()
        .replace(/_/g, ' ')
        .split(' ')
        .map(word => word.charAt(0).toUpperCase() + word.slice(1))
        .join(' ');
    }

    switch (event.monsterSubType) {
      case MonsterSubType.FIRE_DRAGON:
        return 'Infernal Dragon';
      case MonsterSubType.WATER_DRAGON:
        return 'Ocean Dragon';
      case MonsterSubType.EARTH_DRAGON:
        return 'Mountain Dragon';
      case MonsterSubType.AIR_DRAGON:
        return 'Cloud Dragon';
      case MonsterSubType.HEXTECH_DRAGON:
        return 'Hextech Dragon';
      case MonsterSubType.CHEMTECH_DRAGON:
        return 'Chemtech Dragon';
      case MonsterSubType.ELDER_DRAGON:
        return 'Elder Dragon';
      default:
        return 'Dragon';
    }
  }

  protected getEventDescription(event: EventDto): string {
    const actor = this.getParticipantByParticipantId(event.mainParticipantId);
    const victim = this.getParticipantByParticipantId(event.victimParticipantId);

    switch (event.type) {

      case EventType.CHAMPION_KILL: {
        let description = `${actor?.summonerName ?? `P${event.mainParticipantId}`} killed ${victim?.summonerName ?? `P${event.victimParticipantId}`}`;

        if (event.shutdownBounty) {
          description += ` (Shutdown +${event.shutdownBounty}g)`;
        }

        if (event.assistingParticipants.length) {
          const assists = event.assistingParticipants
            .map(id => this.getParticipantByParticipantId(id)?.summonerName ?? `P${id}`)
            .join(', ');

          description += `. Assisted by ${assists}`;
        }

        return description;
      }

      case EventType.ELITE_MONSTER_KILL:
        return `${actor?.summonerName ?? `P${event.mainParticipantId}`} killed ${this.formatMonster(event)}`;

      case EventType.BUILDING_KILL:
        if (event.buildingType === BuildingType.INHIBITOR_BUILDING) {
          return `${actor?.summonerName ?? `P${event.mainParticipantId}`} destroyed the ${this.formatLane(event.laneType)} inhibitor`;
        }
        return `${actor?.summonerName ?? `P${event.mainParticipantId}`} destroyed the ${this.formatLane(event.laneType)} ${this.formatTower(event.towerType)}`;

      case EventType.TURRET_PLATE_DESTROYED: {
              const lane = this.formatLane(event.laneType);
              const actorName = actor?.summonerName ?? (event.mainParticipantId ? `P${event.mainParticipantId}` : 'The team');
              return lane 
                ? `${actorName} destroyed a ${lane.toLowerCase()} turret plate`
                : `${actorName} destroyed a turret plate`;
            }

      case EventType.ITEM_PURCHASED:
        return `${actor?.summonerName ?? `P${event.mainParticipantId}`} purchased ${event.item?.name}`;

      case EventType.ITEM_SOLD:
        return `${actor?.summonerName ?? `P${event.mainParticipantId}`} sold ${event.item?.name}`;

      case EventType.ITEM_DESTROYED:
        return `${actor?.summonerName ?? `P${event.mainParticipantId}`} consumed ${event.item?.name}`;

      case EventType.ITEM_UNDO:
        return `${actor?.summonerName ?? `P${event.mainParticipantId}`} undid purchase (${event.beforeItem?.name} → ${event.afterItem?.name})`;

      case EventType.GAME_END:
        return `Team ${event.teamId} won the game`;

      default:
        return event.type;
    }
  }

  protected getTeamName(teamId?: number): string {
    if (teamId === 100) return 'Blue Team';
    if (teamId === 200) return 'Red Team';
    return teamId ? `Team ${teamId}` : 'Team';
  }

  protected getActorName(participantId?: number | null, teamId?: number): string {
    if (!participantId) {
      return this.getTeamName(teamId);
    }
    
    const participant = this.getParticipantByParticipantId(participantId);
    return participant?.summonerName ?? `P${participantId}`;
  }

  protected getParticipantTeamId(participantId?: number | null): number | undefined {
    if (!participantId) return undefined;

    return this.scoreboard()
      ?.teams
      .find(team => team.participants.some(p => p.participantId === participantId))
      ?.teamId;
  }

  protected getActorColorClass(participantId?: number | null, teamId?: number): string {
    const resolvedTeamId = this.getParticipantTeamId(participantId) ?? teamId;

    if (resolvedTeamId === 100) return 'text-blue-400';
    if (resolvedTeamId === 200) return 'text-red-400';
    return '';
  }

  protected getTeams() {
    return this.scoreboard()?.teams ?? [];
  }

  protected mapX(x: number): number {
    return (x / this.MAP_SIZE) * 100;
  }

  protected mapY(y: number): number {
    return 100 - (y / this.MAP_SIZE) * 100;
  }
}