import { QueueType } from '../enums/queue-type';

export const QueueTypeLabels: Record<QueueType, string> = {
  [QueueType.DRAFT_PICK]: 'Normal Draft',
  [QueueType.RANKED_SOLO]: 'Ranked Solo/Duo',
  [QueueType.RANKED_FLEX]: 'Ranked Flex',
};