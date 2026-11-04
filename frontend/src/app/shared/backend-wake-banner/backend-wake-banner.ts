import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { BackendStatus, BackendWarmup } from '../backend-warmup/backend-warmup';


@Component({
  selector: 'app-backend-wake-banner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './backend-wake-banner.html',
})
export class BackendWakeBanner {
  private readonly warmup = inject(BackendWarmup);

  protected readonly status = this.warmup.status;
  private readonly dismissedStatus = signal<BackendStatus | null>(null);

  protected readonly isError = computed(() => this.status() === 'unreachable');
  protected readonly visible = computed(
    () =>
      (this.status() === 'waking' || this.status() === 'unreachable') &&
      this.dismissedStatus() !== this.status(),
  );

  protected dismiss(): void {
    this.dismissedStatus.set(this.status());
  }

  protected reload(): void {
    window.location.reload();
  }
}