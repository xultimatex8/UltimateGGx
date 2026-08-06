import { ItemDto } from '../item/item.model';

export class ItemsUtil {
  static sortForDisplay(items: ItemDto[]): ItemDto[] {
    if (!items) return [];
    return [...items].sort((a, b) => {
      const aIsFree = a.buyPrice === 0 ? 1 : 0;
      const bIsFree = b.buyPrice === 0 ? 1 : 0;
      return aIsFree - bIsFree;
    });
  }
}