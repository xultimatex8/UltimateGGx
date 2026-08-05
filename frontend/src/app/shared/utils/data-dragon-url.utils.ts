export class DataDragonUrlUtil {
  static championIcon(version: string, riotId: string): string {
    return `https://ddragon.leagueoflegends.com/cdn/${version}/img/champion/${riotId}.png`;
  }

  static itemIcon(version: string, key: number): string {
    return `https://ddragon.leagueoflegends.com/cdn/${version}/img/item/${key}.png`;
  }

  static spellIcon(version: string, riotId: string): string {
    return `https://ddragon.leagueoflegends.com/cdn/${version}/img/spell/${riotId}.png`;
  }
}