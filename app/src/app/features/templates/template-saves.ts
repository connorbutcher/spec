import { computed, Injectable, signal } from '@angular/core';
import { ApiErrorMessage } from './api-error-message';

/**
 * Every save the templates screen makes, whether to a template or a cell type: whether one is running,
 * and the last one's error. Both stores save through here, so the screen shows one busy state and one
 * error however the work is split between them. Provided by the templates page.
 */
@Injectable()
export class TemplateSaves {
  public readonly isSaving = computed(() => this.pending() > 0);

  /** The last save's error, shown until dismissed or the next save. */
  public readonly error = signal<string | null>(null);

  private readonly pending = signal(0);

  public dismissError(): void {
    this.error.set(null);
  }

  /** Runs a save. Its result, or null when it failed, in which case `error` says why. */
  public async run<T>(action: () => Promise<T>): Promise<T | null> {
    this.error.set(null);
    this.pending.update((count) => count + 1);
    try {
      return await action();
    } catch (error) {
      this.error.set(ApiErrorMessage.apiErrorMessage(error));
      return null;
    } finally {
      this.pending.update((count) => count - 1);
    }
  }
}
