import { ChampionDto } from "../shared/champion/champion.model";
import { BuildingType, TowerType } from "../shared/enums/building-type";
import { EventType } from "../shared/enums/event-type";
import { LaneType } from "../shared/enums/lane-type";
import { MonsterType, MonsterSubType } from "../shared/enums/monster-type";
import { ItemDto } from "../shared/item/item.model";
import { RuneDto } from "../shared/rune/rune.model";
import { SummonerSpellDto } from "../shared/summoner-spell/summoner-spell.model";

export interface TimelineDto {
  events: EventDto[];
}

export interface EventDto {
  timestamp: number;

  bounty?: number;
  shutdownBounty?: number;

  monsterType?: MonsterType;
  monsterSubType?: MonsterSubType;

  buildingType?: BuildingType;
  laneType?: LaneType;
  towerType?: TowerType;

  teamId?: number;

  type: EventType;

  mainParticipantId?: number;
  victimParticipantId?: number;
  assistingParticipants: number[];

  item?: ItemDto;
  beforeItem?: ItemDto;
  afterItem?: ItemDto;
}

export interface ScoreboardDto {
  teams: ScoreboardTeamDto[];
  timestamp: number;
}

export interface ScoreboardTeamDto {
  teamId: number;
  participants: ScoreboardParticipantDto[];
}

export interface ScoreboardParticipantDto {
  participantId: number;
  summonerName: string;
  assists: number;
  championLevel: number;
  deaths: number;
  currentGold: number;
  totalGold: number;
  kills: number;
  lane: string;
  minions: number;
  positionX: number;
  positionY: number;
  primaryRune: RuneDto;
  secondaryTree: RuneDto;
  champion: ChampionDto;
  summonerSpells: SummonerSpellDto[];
  items: ItemDto[];
}