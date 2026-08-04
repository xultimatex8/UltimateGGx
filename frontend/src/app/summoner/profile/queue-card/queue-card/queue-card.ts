import { Component, computed, input } from '@angular/core';
import { QueueType } from '../../../../shared/enums/queue-type';
import { QueueDto } from '../../../summoner.model';

const TIER_COLORS: Record<string, string> = {
  IRON: '#6b6560',
  BRONZE: '#a9744f',
  SILVER: '#9ba3ab',
  GOLD: '#fbbf24',
  PLATINUM: '#3fc1b0',
  EMERALD: '#33b679',
  DIAMOND: '#5b9bd5',
  MASTER: '#c060e0',
  GRANDMASTER: '#e05252',
  CHALLENGER: '#f4c95d',
};

const QUEUE_LABELS: Record<QueueType, string> = {
  [QueueType.DRAFT_PICK]: 'Draft Pick',
  [QueueType.RANKED_SOLO]: 'Ranked Solo/Duo',
  [QueueType.RANKED_FLEX]: 'Ranked Flex',
};

@Component({
  selector: 'app-queue-card',
  imports: [],
  templateUrl: './queue-card.html',
})
export class QueueCard {
  type = input.required<QueueType>();
  queue = input<QueueDto | null>(null);

  label = computed(() => QUEUE_LABELS[this.type()]);

  winRate = computed(() => {
    const q = this.queue();
    if (!q) return 0;
    const total = q.wins + q.losses;
    return total === 0 ? 0 : Math.round((q.wins / total) * 100);
  });

  tierColor = computed(() => {
    const tier = this.queue()?.tier?.toUpperCase();
    return tier ? (TIER_COLORS[tier] ?? '#9ca3af') : '#9ca3af';
  });
}