import { Component, input } from '@angular/core';
import { Rune } from '../rune/rune';
import { SummonerSpell } from '../summoner-spell/summoner-spell';
import { ParticipantIdentityDto } from './participan-identity.model';
import { ParticipantsUtil } from '../utils/participants.util';
import { DataDragonUrlUtil } from '../utils/data-dragon-url.utils';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-participant-identity',
  imports: [Rune, SummonerSpell, RouterLink],
  templateUrl: './participant-identity.html',
})
export class ParticipantIdentity {
  participant = input.required<ParticipantIdentityDto>();
  ddragonVersion = input.required<string>();

  linkToProfile = input(false);
  highlightPuuid = input<string | null>(null);

  avatarSize = input('w-8 h-8');
  laneIconSize = input('w-2.5 h-2.5');
  levelTextSize = input('text-[9px]');
  spellSize = input('w-3.5 h-3.5');
  primaryRuneSize = input('w-4 h-4');
  secondaryRuneSize = input('w-3 h-3');
  nameSize = input('text-xs');

  protected readonly ParticipantsUtil = ParticipantsUtil;

  protected championIconUrl(riotId: string): string {
    return DataDragonUrlUtil.championIcon(this.ddragonVersion(), riotId);
  }

  protected isHighlighted(): boolean {
    const puuid = this.highlightPuuid();
    return !!puuid && this.participant().puuid === puuid;
  }
}