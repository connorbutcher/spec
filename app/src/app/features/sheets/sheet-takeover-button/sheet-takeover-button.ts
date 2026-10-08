import { httpResource } from '@angular/common/http';
import { Component, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { RowTakeoverAvailability } from '../models/row-takeover-availability.model';
import { RowTakeover } from '../models/row-takeover.model';
import { SheetRow } from '../models/sheet-row.model';
import { RowTakeoverStore } from '../row-takeover.store';

/**
 * For a row checked out to someone else: asks them to hand it over. While the request waits it shows
 * how long until it is granted unanswered, and lets the viewer withdraw it.
 *
 * A row its holder has changed can't be taken over until they publish, and only the server can see
 * someone else's unpublished changes, so the button asks the server whether it applies and shows the
 * reason in its place when it doesn't.
 */
@Component({
  selector: 'app-sheet-takeover-button',
  imports: [ButtonModule, TooltipModule],
  templateUrl: './sheet-takeover-button.html',
  styleUrl: './sheet-takeover-button.scss',
})
export class SheetTakeoverButton {
  public readonly row = input.required<SheetRow>();

  /** The viewer's request for this row, while it waits for an answer. */
  public readonly waiting = computed<RowTakeover | null>(
    () => this.takeovers.outgoingByRow().get(this.row().id) ?? null,
  );

  public readonly secondsLeft = computed(() => {
    const waiting = this.waiting();
    return waiting === null ? 0 : this.takeovers.secondsLeft(waiting);
  });

  /** Why the row can't be asked for, or null when it can or the answer isn't in yet. */
  public readonly blockedReason = computed<string | null>(() => {
    const availability = this.availability.hasValue() ? this.availability.value() : null;
    return availability !== null && !availability.isAvailable ? availability.reason : null;
  });

  /** Nothing can be asked for until the server has said the row can be. */
  public readonly canRequest = computed(
    () =>
      this.availability.hasValue() && this.availability.value().isAvailable && !this.isSending(),
  );

  public readonly isSending = signal(false);

  private readonly takeovers = inject(RowTakeoverStore);

  private readonly availability = httpResource<RowTakeoverAvailability>(
    () => `/api/sheet-rows/${this.row().id}/takeover-availability`,
  );

  constructor() {
    // The row is a new object whenever anything about it changes (who holds it, a publish), and its
    // availability may have changed with it. A different row is already re-read by the resource itself.
    let lastRow: SheetRow | null = null;
    effect(() => {
      const row = this.row();
      if (lastRow !== null && lastRow.id === row.id) {
        untracked(() => this.availability.reload());
      }
      lastRow = row;
    });
  }

  public async request(): Promise<void> {
    this.isSending.set(true);
    try {
      await this.takeovers.request(this.row().id);
    } finally {
      this.isSending.set(false);
      // A refusal usually means the row changed under the viewer; show the current reason.
      this.availability.reload();
    }
  }

  public withdraw(takeover: RowTakeover): void {
    void this.takeovers.cancel(takeover);
  }
}
