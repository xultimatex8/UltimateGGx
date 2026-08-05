import { Component, effect, inject, input, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Match } from '../match';
import { MatchDto } from '../match.model';
import { DataDragon } from '../../../shared/data-dragon/data-dragon';
import { QueueTypeLabels } from '../../../shared/utils/queue-type.util';
import { MatchParticipantsUtil } from '../../../shared/utils/match-participants.util';
import { RouterLink } from '@angular/router';
import { Item } from '../../../shared/item/item';
import { SummonerSpell } from '../../../shared/summoner-spell/summoner-spell';
import { Rune } from '../../../shared/rune/rune';
import { FormatDurationUtil } from '../../../shared/utils/format-duration.util';
import { ItemDto } from '../../../shared/item/item.model';
import { QueueType } from '../../../shared/enums/queue-type';

@Component({
  selector: 'app-match-history',
  imports: [DatePipe, RouterLink, Item, SummonerSpell, Rune],
  templateUrl: './match-history.html',
})
export class MatchHistory {
  puuid = input.required<string>();

  private matchService = inject(Match);
  private dataDragon = inject(DataDragon);

  protected readonly QueueTypeLabels = QueueTypeLabels;
  protected readonly MatchParticipantsUtil = MatchParticipantsUtil;
  protected readonly FormatDurationUtil = FormatDurationUtil;
  protected readonly ddragonVersion = this.dataDragon.version();

  matches = signal<MatchDto[]>([]);
  queueType = signal<QueueType>(QueueType.DRAFT_PICK);
  loading = signal(false);
  error = signal<string | null>(null);

  queueTypes = Object.values(QueueType);

  constructor() {
    effect(() => {
      const puuid = this.puuid();
      const queueType = this.queueType();

      if (puuid && queueType) {
        this.loadMatches();
      }
    });
  }

  protected championIconUrl(riotId: string): string {
    return `https://ddragon.leagueoflegends.com/cdn/${this.ddragonVersion}/img/champion/${riotId}.png`;
  }

  protected runeIconUrl(runeIcon: string): string {
    return `https://ddragon.leagueoflegends.com/cdn/img/${runeIcon}`;
  }

  protected matchResult(match: MatchDto): string {
    if (match.endOfGameResult !== 'GameComplete') {
      return match.endOfGameResult;
    }

    return match.win ? 'Victory' : 'Defeat';
  }

  protected matchResultClass(match: MatchDto): string {
    if (match.endOfGameResult !== 'GameComplete') {
      return 'bg-warning-surface text-warning-border';
    }

    return match.win
      ? 'bg-success-surface text-success-border'
      : 'bg-danger-surface text-danger-border';
  }

  protected matchBorderClass(match: MatchDto): string {
    if (match.endOfGameResult !== 'GameComplete') {
      return 'bg-warning-border';
    }

    return match.win
      ? 'bg-success-border'
      : 'bg-danger-border';
  }

  protected sortedItems(items: ItemDto[]): ItemDto[] {
    return [...items].sort((a, b) => {
      const aIsFree = a.buyPrice === 0 ? 1 : 0;
      const bIsFree = b.buyPrice === 0 ? 1 : 0;
      return aIsFree - bIsFree;
    });
  }

  fetchMatches(): void {
    this.loading.set(true);
    this.error.set(null);

    this.matchService
      .fetchSummonerMatches(this.puuid(), this.queueType())
      .subscribe({
        next: () => this.loadMatches(),
        error: () => {
          this.error.set("Couldn't fetch matches.");
          this.loading.set(false);
        },
      });
  }

  loadMatches(): void {
    this.matchService
      .getSummonerMatches(this.puuid(), this.queueType())
      .subscribe({
        next: (result) => {
          this.matches.set(result.items);
          this.loading.set(false);
        },
        error: () => {
          this.error.set("Couldn't load match history.");
          this.loading.set(false);
        },
      });
  }
}