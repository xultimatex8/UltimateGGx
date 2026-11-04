import { Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';

export type BackendStatus = 'checking' | 'waking' | 'ready' | 'unreachable';

const HEALTH_URL = `${environment.apiUrl}/api/health`;

const SHOW_AFTER_MS = 1500;
const REQUEST_TIMEOUT_MS = 8000;
const RETRY_DELAY_MS = 3000;
const GIVE_UP_AFTER_MS = 90_000;

const sleep = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

@Injectable({ providedIn: 'root' })
export class BackendWarmup {
  private readonly _status = signal<BackendStatus>('checking');
  readonly status = this._status.asReadonly();

  private promise?: Promise<boolean>;

  start(): Promise<boolean> {
    return (this.promise ??= this.run());
  }

  private async run(): Promise<boolean> {
    const startedAt = Date.now();

    const wakingTimer = setTimeout(() => {
      if (this._status() === 'checking') this._status.set('waking');
    }, SHOW_AFTER_MS);

    while (Date.now() - startedAt < GIVE_UP_AFTER_MS) {
      if (await this.ping()) {
        clearTimeout(wakingTimer);
        this._status.set('ready');
        return true;
      }
      await sleep(RETRY_DELAY_MS);
    }

    clearTimeout(wakingTimer);
    this._status.set('unreachable');
    return false;
  }

  private async ping(): Promise<boolean> {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);
    try {
      const res = await fetch(HEALTH_URL, {
        signal: controller.signal,
        cache: 'no-store',
      });
      return res.ok;
    } catch {
      return false;
    } finally {
      clearTimeout(timeout);
    }
  }
}