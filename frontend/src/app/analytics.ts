import { inject } from '@vercel/analytics';

export function initializeAnalytics(): void {
  inject();
}