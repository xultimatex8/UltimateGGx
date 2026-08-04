import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { Summoner } from '../../summoner';
import { SummonerDto } from '../../summoner.model';
import { QueueType } from '../../../shared/enums/queue-type';
import { QueueCard } from '../queue-card/queue-card/queue-card';
import { DataDragon } from '../../../shared/data-dragon/data-dragon';

@Component({
  selector: 'app-summoner-profile',
  imports: [DatePipe, QueueCard],
  templateUrl: './summoner-profile.html',
})
export class SummonerProfile {
  private route = inject(ActivatedRoute);
  private summonerService = inject(Summoner);
  private dataDragon = inject(DataDragon);

  protected readonly ddragonVersion = this.dataDragon.version();
  protected readonly QueueType = QueueType;

  summoner = signal<SummonerDto | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  soloQueue = computed(
    () => this.summoner()?.queues.find((q) => q.type === QueueType.RANKED_SOLO) ?? null
  );
  flexQueue = computed(
    () => this.summoner()?.queues.find((q) => q.type === QueueType.RANKED_FLEX) ?? null
  );

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const username = params.get('username');
      const tag = params.get('tag');

      if (username && tag) {
        this.fetchSummoner(username, tag);
      }
    });
  }

  protected profileIconUrl(iconId: number): string {
    return `https://ddragon.leagueoflegends.com/cdn/${this.ddragonVersion}/img/profileicon/${iconId}.png`;
  }

  private fetchSummoner(username: string, tag: string) {
    this.loading.set(true);
    this.error.set(null);

    this.summonerService.getSummoner(username, tag).subscribe({
      next: (summoner) => {
        this.summoner.set(summoner);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("We couldn't find that summoner.");
        this.loading.set(false);
      },
    });
  }
}