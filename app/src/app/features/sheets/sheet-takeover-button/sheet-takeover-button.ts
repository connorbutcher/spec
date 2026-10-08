import { Component, computed, inject, input, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { RowTakeover } from '../models/row-takeover.model';
import { SheetRow } from '../models/sheet-row.model';
import { RowTakeoverStore } from '../row-takeover.store';

/**
 * For a row checked out to someone else: asks them to hand it over. While the request waits it shows
 * how long until it is granted unanswered, and lets the viewer withdraw it.
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

  public readonly isSending = signal(false);

  private readonly takeovers = inject(RowTakeoverStore);

  public async request(): Promise<void> {
    this.isSending.set(true);
    try {
      await this.takeovers.request(this.row().id);
    } finally {
      this.isSending.set(false);
    }
  }

  public withdraw(takeover: RowTakeover): void {
    void this.takeovers.cancel(takeover);
  }
}
