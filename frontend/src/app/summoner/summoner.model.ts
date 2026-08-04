import { QueueType } from "../shared/enums/queue-type";

export interface QueueDto {
  type: QueueType;
  tier: string;
  rank: string;
  points: number;
  wins: number;
  losses: number;
}

export interface SummonerDto {
  puuid: string;
  username: string;
  tag: string;
  level: number;
  profileIconId: number;
  lastUpdate: string;
  queues: QueueDto[];
}