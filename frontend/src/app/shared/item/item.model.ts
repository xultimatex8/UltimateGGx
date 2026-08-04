export interface ItemDto {
  key: number;
  name: string;
  description: string;
  buyPrice: number;
  sellPrice: number;
  stats: Record<string, number>;
}