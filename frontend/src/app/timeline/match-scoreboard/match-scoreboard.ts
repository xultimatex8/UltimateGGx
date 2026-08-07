import { Component, computed, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { EventDto, ScoreboardDto } from '../timeline.model';
import { ParticipantIdentity } from '../../shared/participant-identity/participant-identity';
import { ParticipantsUtil } from '../../shared/utils/participants.util';
import { ItemSlots } from '../../shared/item-slots/item-slots';

@Component({
  selector: 'app-match-scoreboard',
  imports: [DecimalPipe, ParticipantIdentity, ItemSlots],
  templateUrl: './match-scoreboard.html',
})
export class MatchScoreboard {
  scoreboard = input<ScoreboardDto | null>(null);
  selectedEvent = input<EventDto | null>(null);
  ddragonVersion = input.required<string>();

  protected blueTeam = computed(() => {
    const sb = this.scoreboard();
    return sb ? ParticipantsUtil.getTeamFromGroups(sb.teams, 100) : [];
  });

  protected redTeam = computed(() => {
    const sb = this.scoreboard();
    return sb ? ParticipantsUtil.getTeamFromGroups(sb.teams, 200) : [];
  });
}