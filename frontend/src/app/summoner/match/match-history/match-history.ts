import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Match } from '../match';
import { MatchDto } from '../match.model';
import { DataDragon } from '../../../shared/data-dragon/data-dragon';
import { QueueTypeLabels } from '../../../shared/utils/queue-type.util';
import { Router, RouterLink } from '@angular/router';
import { Item } from '../../../shared/item/item';
import { SummonerSpell } from '../../../shared/summoner-spell/summoner-spell';
import { Rune } from '../../../shared/rune/rune';
import { FormatDurationUtil } from '../../../shared/utils/format-duration.util';
import { ItemDto } from '../../../shared/item/item.model';
import { QueueType } from '../../../shared/enums/queue-type';
import { ParticipantsUtil } from '../../../shared/utils/participants.util';
import { ItemSlots } from '../../../shared/item-slots/item-slots';
import { ParticipantIdentity } from '../../../shared/participant-identity/participant-identity';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-match-history',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    Item,
    SummonerSpell,
    Rune,
    ItemSlots,
    ParticipantIdentity
  ],
  templateUrl: './match-history.html',
})
export class MatchHistory {
  puuid = input.required<string>();

  private matchService = inject(Match);
  private dataDragon = inject(DataDragon);
  private router = inject(Router);

  protected readonly QueueTypeLabels = QueueTypeLabels;
  protected readonly ParticipantsUtil = ParticipantsUtil;
  protected readonly FormatDurationUtil = FormatDurationUtil;
  protected readonly ddragonVersion = this.dataDragon.version();

  matches = signal<MatchDto[]>([]);
  queueType = signal<QueueType>(QueueType.DRAFT_PICK);
  page = signal(1);
  loading = signal(false);
  error = signal<string | null>(null);
  expandedMatches = signal<Set<number>>(new Set());

  queueTypes = Object.values(QueueType);
  pageSize = 10;

  totalItems = signal(0);
  totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalItems() / this.pageSize))
  );

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
      return 'bg-warning-border hover:rounded-lg';
    }

    return match.win
      ? 'bg-success-border'
      : 'bg-danger-border';
  }

  protected toggleMatch(index: number): void {
    this.expandedMatches.update((expanded) => {
      const updated = new Set(expanded);
      if (updated.has(index)) {
        updated.delete(index);
      } else {
        updated.add(index);
      }
      return updated;
    });
  }

  protected isExpanded(index: number): boolean {
    return this.expandedMatches().has(index);
  }

  protected sortedItems(items: ItemDto[]): ItemDto[] {
    return [...items].sort((a, b) => {
      const aIsFree = a.buyPrice === 0 ? 1 : 0;
      const bIsFree = b.buyPrice === 0 ? 1 : 0;
      return aIsFree - bIsFree;
    });
  }

  goToTimeline(matchId: string) {
    this.router.navigate(['/match', matchId]);
  }

  fetchMatches(): void {
    this.loading.set(true);
    this.error.set(null);

    this.matchService
      .fetchSummonerMatches(this.puuid(), this.queueType())
      .subscribe({
        next: () => this.loadMatches(),
        error: (err: HttpErrorResponse) => {
          this.error.set(err.error?.error ?? 'An unexpected error occurred.');
          this.loading.set(false);
        },
      });
  }

  loadMatches(): void {
    this.matchService
      .getSummonerMatches(
        this.puuid(),
        this.queueType(),
        this.page(),
        this.pageSize
      )
      .subscribe({
        next: (result) => {
          this.matches.set(result.items);
          this.totalItems.set(result.totalItems);
          this.expandedMatches.set(new Set());
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(err.error?.error ?? 'An unexpected error occurred.');
          this.loading.set(false);
        },
      });
  }

  changeQueue(type: QueueType) {
    this.page.set(1);
    this.queueType.set(type);
  }

  nextPage() {
    if (this.page() < this.totalPages()) {
      this.page.update(p => p + 1);
      this.loadMatches();
    }
  }

  previousPage() {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.loadMatches();
    }
  }

  getMaxDamage(match: MatchDto): number {
    return Math.max(...match.participants.map(p => p.damageToChampions || 0), 1);
  }

  getParticipantByPuuid(match: MatchDto, puuid: string) {
    return match.participants.find(
      p => p.puuid === puuid
    )
  }
}