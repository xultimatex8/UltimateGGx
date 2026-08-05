import { ChampionDto } from "../../shared/champion/champion.model";
import { QueueType } from "../../shared/enums/queue-type";
import { ItemDto } from "../../shared/item/item.model";
import { RuneDto } from "../../shared/rune/rune.model";
import { SummonerSpellDto } from "../../shared/summoner-spell/summoner-spell.model";

export interface MatchDto {
  matchId: string;
  endOfGameResult: string;
  gameDuration: number;
  gameEndTimestamp: number;
  queueType: QueueType;
  win: boolean;
  participants: ParticipantDetailDto[];
}

export interface ParticipantDetailDto {
  puuid: string;
  summonerName: string;
  summonerTag: string;
  assists: number;
  championLevel: number;
  deaths: number;
  gold: number;
  kills: number;
  lane: string;
  minions: number;
  damageToChampions: number;
  teamId: number;
  primaryRune: RuneDto;
  secondaryTree: RuneDto;
  champion: ChampionDto;
  items: ItemDto[];
  summonerSpells: SummonerSpellDto[];
}