import { MatchDto, ParticipantDetailDto } from "../../summoner/match/match.model";

const LANE_ORDER = [
  'TOP',
  'JUNGLE',
  'MIDDLE',
  'BOTTOM',
  'UTILITY'
];

const LANE_LABELS: Record<string, string> = {
  TOP: 'TOP',
  JUNGLE: 'JUNGLE',
  MIDDLE: 'MIDDLE',
  BOTTOM: 'BOTTOM',
  UTILITY: 'SUPPORT'
};

export class MatchParticipantsUtil {

  static getBlueTeam(match: MatchDto): ParticipantDetailDto[] {
    return this.getTeamParticipants(match, 100);
  }

  static getRedTeam(match: MatchDto): ParticipantDetailDto[] {
    return this.getTeamParticipants(match, 200);
  }

  static getTeamParticipants(
    match: MatchDto,
    teamId: number
  ): ParticipantDetailDto[] {
    return this.sortByLane(
      match.participants.filter(participant => participant.teamId === teamId)
    );
  }

  static getParticipant(
    match: MatchDto,
    puuid: string
  ): ParticipantDetailDto | undefined {
    return match.participants.find(participant => participant.puuid === puuid);
  }

  static sortByLane(
    participants: ParticipantDetailDto[]
  ): ParticipantDetailDto[] {
    return [...participants].sort(
      (a, b) =>
        this.getLaneOrder(a.lane) - this.getLaneOrder(b.lane)
    );
  }

  private static getLaneOrder(lane: string): number {
    const index = LANE_ORDER.indexOf(lane.toUpperCase());

    return index === -1 ? Number.MAX_SAFE_INTEGER : index;
  }
}