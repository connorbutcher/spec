import { Component, computed, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { RowTakeover } from '../models/row-takeover.model';
import { RowTakeoverStore } from '../row-takeover.store';
import { rowLabel } from '../sheet-labels.util';
import { SheetStore } from '../sheet.store';

/**
 * Someone is asking for a row that is checked out to the viewer. The viewer hands it over or keeps it;
 * left unanswered, it is handed over when the countdown ends. Changing the row in the meantime keeps it
 * (the server closes the request).
 */
@Component({
  selector: 'app-sheet-takeover-request',
  imports: [ButtonModule, MessageModule],
  templateUrl: './sheet-takeover-request.html',
  styleUrl: './sheet-takeover-request.scss',
})
export class SheetTakeoverRequest {
  public readonly takeover = input.required<RowTakeover>();

  public readonly secondsLeft = computed(() => this.takeovers.secondsLeft(this.takeover()));

  /** The row by the first text typed into it, so the viewer knows which one is meant. */
  public readonly rowName = computed(() => {
    const row = this.store.index().rows.get(this.takeover().rowId);
    const label = row ? rowLabel(row) : null;
    return label ? `the row "${label}"` : 'a row';
  });

  private readonly takeovers = inject(RowTakeoverStore);
  private readonly store = inject(SheetStore);

  public showRow(): void {
    this.store.selectRow(this.takeover().rowId);
  }

  public handOver(): void {
    void this.takeovers.approve(this.takeover());
  }

  public keep(): void {
    void this.takeovers.deny(this.takeover());
  }
}
