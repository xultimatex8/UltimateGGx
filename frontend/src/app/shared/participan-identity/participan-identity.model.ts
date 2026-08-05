import { ChampionDto } from '../champion/champion.model';
import { RuneDto } from '../rune/rune.model';
import { SummonerSpellDto } from '../summoner-spell/summoner-spell.model';

export interface ParticipantIdentityDto {
  champion: ChampionDto;
  championLevel: number;
  lane: string;
  summonerSpells: SummonerSpellDto[];
  primaryRune: RuneDto;
  secondaryTree: RuneDto;
  summonerName: string;
  summonerTag?: string;
  puuid?: string;
}