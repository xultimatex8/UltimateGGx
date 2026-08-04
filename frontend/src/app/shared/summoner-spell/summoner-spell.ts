import { Component, computed, input } from '@angular/core';
import { SummonerSpellDto } from './summoner-spell.model';

@Component({
  selector: 'app-summoner-spell',
  imports: [],
  templateUrl: './summoner-spell.html',
})
export class SummonerSpell {
  spell = input<SummonerSpellDto | null>(null);
  ddragonVersion = input.required<string>();
  size = input<string>('w-5 h-5');

  protected iconUrl = computed(() => {
    const spell = this.spell();
    return spell ? this.buildIconUrl(this.ddragonVersion(), spell.riotId) : null;
  });

  private buildIconUrl(ddragonVersion: string, riotId: string): string {
    return `https://ddragon.leagueoflegends.com/cdn/${ddragonVersion}/img/spell/${riotId}.png`;
  }
}