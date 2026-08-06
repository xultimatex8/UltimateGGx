import { Component, input } from '@angular/core';
import { Rune } from '../rune/rune';
import { SummonerSpell } from '../summoner-spell/summoner-spell';
import { ParticipantIdentityDto } from './participant-identity.model';
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

  avatarSize = input('w-8 h-8 lg:w-9 lg:h-9');
  laneIconSize = input('w-2.5 h-2.5 lg:w-3 lg:h-3');
  levelTextSize = input('text-[9px] lg:text-[10px]');
  spellSize = input('w-3.5 h-3.5 lg:w-4 lg:h-4');
  primaryRuneSize = input('w-3 h-3 lg:w-4 lg:h-4');
  secondaryRuneSize = input('w-3 h-3 lg:w-3.5 lg:h-3.5');
  nameSize = input('text-[11px] md:text-xs lg:sm');

  protected readonly ParticipantsUtil = ParticipantsUtil;

  protected championIconUrl(riotId: string): string {
    return DataDragonUrlUtil.championIcon(this.ddragonVersion(), riotId);
  }

  protected isHighlighted(): boolean {
    const puuid = this.highlightPuuid();
    return !!puuid && this.participant().puuid === puuid;
  }
}