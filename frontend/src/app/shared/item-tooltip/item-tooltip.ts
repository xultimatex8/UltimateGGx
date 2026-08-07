import { Component, input } from '@angular/core';
import { ItemDto } from '../item/item.model';

@Component({
  selector: 'app-item-tooltip',
  templateUrl: './item-tooltip.html'
})
export class ItemTooltip {
  item = input.required<ItemDto>();
  stats = input<any[]>([]);
}