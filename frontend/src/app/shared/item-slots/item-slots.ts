import { Component, computed, input } from '@angular/core';
import { Item } from '../item/item';
import { ItemDto } from '../item/item.model';
import { ItemsUtil } from '../utils/items.util';

@Component({
  selector: 'app-item-slots',
  standalone: true,
  imports: [Item],
  templateUrl: './item-slots.html',
})
export class ItemSlots {
  items = input.required<ItemDto[]>();
  ddragonVersion = input.required<string>();
  size = input('w-5.5 h-5.5 lg:w-6.5 lg:h-6.5');

  protected readonly slots = [0, 1, 2, 3, 4, 5];

  protected sorted = computed(() => ItemsUtil.sortForDisplay(this.items()));
  protected mainItems = computed(() => {
    const all = this.sorted();
    return all.filter(item => item.buyPrice !== 0);
  });
  protected trinket = computed(() => {
    return this.sorted().find(item => item.buyPrice === 0) ?? null;
  });
}