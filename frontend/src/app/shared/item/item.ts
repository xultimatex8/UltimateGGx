import { Component, computed, input } from '@angular/core';
import { ItemDto } from './item.model';

const STAT_LABELS: Record<string, string> = {
  FlatHPPoolMod: 'Health',
  rFlatHPModPerLevel: 'Health per Level',
  FlatMPPoolMod: 'Mana',
  rFlatMPModPerLevel: 'Mana per Level',
  PercentHPPoolMod: 'Health',
  PercentMPPoolMod: 'Mana',
  FlatHPRegenMod: 'Health Regen',
  rFlatHPRegenModPerLevel: 'Health Regen per Level',
  PercentHPRegenMod: 'Health Regen',
  FlatMPRegenMod: 'Mana Regen',
  rFlatMPRegenModPerLevel: 'Mana Regen per Level',
  PercentMPRegenMod: 'Mana Regen',
  FlatArmorMod: 'Armor',
  rFlatArmorModPerLevel: 'Armor per Level',
  PercentArmorMod: 'Armor',
  FlatPhysicalDamageMod: 'Attack Damage',
  rFlatPhysicalDamageModPerLevel: 'Attack Damage per Level',
  PercentPhysicalDamageMod: 'Attack Damage',
  FlatSpellBlockMod: 'Magic Resist',
  rFlatSpellBlockModPerLevel: 'Magic Resist per Level',
  PercentSpellBlockMod: 'Magic Resist',
  FlatMagicDamageMod: 'Ability Power',
  rFlatMagicDamageModPerLevel: 'Ability Power per Level',
  PercentMagicDamageMod: 'Ability Power',
  FlatMovementSpeedMod: 'Movement Speed',
  rFlatMovementSpeedModPerLevel: 'Movement Speed per Level',
  PercentMovementSpeedMod: 'Movement Speed',
  FlatAttackSpeedMod: 'Attack Speed',
  PercentAttackSpeedMod: 'Attack Speed',
  rPercentAttackSpeedModPerLevel: 'Attack Speed per Level',
  FlatCritChanceMod: 'Critical Strike Chance',
  PercentCritChanceMod: 'Critical Strike Chance',
  FlatCritDamageMod: 'Critical Strike Damage',
  PercentCritDamageMod: 'Critical Strike Damage',
  FlatBlockMod: 'Block',
  PercentBlockMod: 'Block',
  FlatSpellDamageMod: 'Spell Damage',
  PercentSpellDamageMod: 'Spell Damage',
  FlatEXPBonus: 'Experience Bonus',
  PercentEXPBonus: 'Experience Bonus',
  FlatEnergyPoolMod: 'Energy',
  rFlatEnergyModPerLevel: 'Energy per Level',
  PercentEnergyPoolMod: 'Energy',
  FlatEnergyRegenMod: 'Energy Regen',
  rFlatEnergyRegenModPerLevel: 'Energy Regen per Level',
  PercentEnergyRegenMod: 'Energy Regen',
  FlatRuneMod: 'Rune',
  PercentRuneMod: 'Rune',
  PercentLifeStealMod: 'Life Steal',
  PercentSpellVampMod: 'Spell Vamp',
};

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
    return item ? this.buildIconUrl(this.ddragonVersion(), item.key) : null;
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

  private buildIconUrl(ddragonVersion: string, key: number): string {
    return `https://ddragon.leagueoflegends.com/cdn/${ddragonVersion}/img/item/${key}.png`;
  }

  private formatStatName(statKey: string): string {
    if (STAT_LABELS[statKey]) {
      return STAT_LABELS[statKey];
    }

    return statKey
      .replace(/^r/, '')
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, (c) => c.toUpperCase())
      .replace(/\bMod\b/gi, '')
      .trim();
  }
}