import { Component, computed, input } from '@angular/core';
import { RuneDto } from './rune.model';

@Component({
  selector: 'app-rune',
  imports: [],
  templateUrl: './rune.html',
})
export class Rune {
  rune = input<RuneDto | null>(null);
  size = input<string>('w-5 h-5');

  protected iconUrl = computed(() => {
    const rune = this.rune();
    return rune ? this.buildIconUrl(rune.icon) : null;
  });

  private buildIconUrl(icon: string): string {
    return `https://ddragon.leagueoflegends.com/cdn/img/${icon}`;
  }
}