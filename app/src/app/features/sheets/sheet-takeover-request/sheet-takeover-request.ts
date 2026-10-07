import { Component, computed, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { RowTakeover } from '../models/row-takeover.model';
import { SheetLiveStore } from '../sheet-live.store';
import { SheetStore } from '../sheet.store';

/**
 * Someone is asking for a row that is checked out to the viewer. The viewer hands it over or keeps it;
 * left unanswered, it is handed over when the countdown ends.
 */
@Component({
  selector: 'app-sheet-takeover-request',
  imports: [ButtonModule, MessageModule],
  templateUrl: './sheet-takeover-request.html',
  styleUrl: './sheet-takeover-request.scss',
})
export class SheetTakeoverRequest {
  public readonly takeover = input.required<RowTakeover>();

  public readonly secondsLeft = computed(() => this.live.secondsLeft(this.takeover()));

  /** The row by the first text typed into it, so the viewer knows which one is meant. */
  public readonly rowName = computed(() => {
    const row = this.store.index().rows.get(this.takeover().rowId);
    const text = row?.cells.map((cell) => cell.textValue).find((value) => !!value);
    return text ? `the row "${text}"` : 'a row';
  });

  private readonly live = inject(SheetLiveStore);
  private readonly store = inject(SheetStore);

  public showRow(): void {
    this.store.selectRow(this.takeover().rowId);
  }

  public handOver(): void {
    void this.live.approve(this.takeover());
  }

  public keep(): void {
    void this.live.deny(this.takeover());
  }
}
