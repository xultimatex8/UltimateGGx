import { MatchDto } from "../../summoner/match/match.model";

const LANE_ORDER = ['TOP', 'JUNGLE', 'MIDDLE', 'BOTTOM', 'UTILITY'];

const LANE_LABELS: Record<string, string> = {
  TOP: 'TOP',
  JUNGLE: 'JUNGLE',
  MIDDLE: 'MIDDLE',
  BOTTOM: 'BOTTOM',
  UTILITY: 'SUPPORT',
};

const LANE_ICONS: Record<string, string> = {
  TOP: 'top-icon.svg',
  JUNGLE: 'jungle-icon.svg',
  MIDDLE: 'middle-icon.svg',
  BOTTOM: 'bottom-icon.svg',
  UTILITY: 'utility-icon.svg',
};

export interface HasLaneAndTeam {
  lane: string;
  teamId?: number;
}

export class ParticipantsUtil {
  static getTeamFromGroups<T extends HasLaneAndTeam>(
    teams: { teamId: number; participants: T[] }[],
    teamId: number
  ): T[] {
    const team = teams.find((t) => t.teamId === teamId);
    return team ? this.sortByLane(team.participants) : [];
  }

  static getTeamFromFlatList<T extends HasLaneAndTeam>(
    participants: T[],
    teamId: number
  ): T[] {
    return this.sortByLane(participants.filter((p) => p.teamId === teamId));
  }

  static sortByLane<T extends HasLaneAndTeam>(participants: T[]): T[] {
    return [...participants].sort(
      (a, b) => this.getLaneOrder(a.lane) - this.getLaneOrder(b.lane)
    );
  }

  private static getLaneOrder(lane: string): number {
    const index = LANE_ORDER.indexOf(lane?.toUpperCase());
    return index === -1 ? Number.MAX_SAFE_INTEGER : index;
  }

  static getLaneLabel(lane: string): string {
    return LANE_LABELS[lane?.toUpperCase()] || 'UNKNOWN LANE';
  }

  static getLaneIcon(lane: string): string {
    return LANE_ICONS[lane?.toUpperCase()] || 'unknown-icon.svg';
  }
}