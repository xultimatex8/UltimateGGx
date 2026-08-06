// shared/item/item.ts (completo)
import { Component, computed, input } from '@angular/core';
import { ItemDto } from './item.model';
import { ITEM_STAT_LABELS } from './item-stat-labels.const';
import { DataDragonUrlUtil } from '../utils/data-dragon-url.utils';

@Component({
  selector: 'app-item',
  imports: [],
  templateUrl: './item.html',
})
export class Item {
  item = input<ItemDto | null>(null);
  ddragonVersion = input.required<string>();
  size = input<string>('w-6 h-6');

  protected iconUrl = computed(() => {
    const item = this.item();
    return item ? DataDragonUrlUtil.itemIcon(this.ddragonVersion(), item.key) : null;
  });

  protected statEntries = computed(() => {
    const item = this.item();
    if (!item) return [];

    return Object.entries(item.stats ?? {}).map(([key, value]) => ({
      label: this.formatStatName(key),
      value: this.formatStatValue(key, value),
    }));
  });

  private formatStatValue(statKey: string, value: number): string {
    if (statKey.toLowerCase().includes('percent')) {
      return `${Math.round(value * 100)}%`;
    }
    return `${value}`;
  }

  private formatStatName(statKey: string): string {
    if (ITEM_STAT_LABELS[statKey]) {
      return ITEM_STAT_LABELS[statKey];
    }

    return statKey
      .replace(/^r/, '')
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, (c) => c.toUpperCase())
      .replace(/\bMod\b/gi, '')
      .trim();
  }
}