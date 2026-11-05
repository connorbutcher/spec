import { Component, computed, inject, input } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { ItemStatusUtil } from '../status/item-status.util';
import { ItemStatus } from '../models/item-status.model';
import { SectionTone } from '../models/section-tone';
import { SheetRow } from '../models/sheet-row.model';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetLabelsUtil } from '../status/sheet-labels.util';
import { SheetStatus } from '../sheet-status/sheet-status';
import { SheetStore } from '../sheet.store';

/**
 * A row's cell in the status column, the first column of every sheet table: the version the row is on,
 * and its state and who has it when it isn't simply published. It stays at the left edge while the
 * table scrolls sideways, is tinted like the rest of its row, and selects the row when clicked.
 *
 * With "Changes since vN" on, the version of a row that changed after vN is written in bold.
 */
@Component({
  selector: 'app-sheet-row-status-cell',
  imports: [SheetStatus, TooltipModule],
  templateUrl: './sheet-row-status-cell.html',
  styleUrl: './sheet-row-status-cell.scss',
  host: {
    class: 'app-grid-cell',
    role: 'gridcell',
    '[class.selected]': 'isSelected()',
    '[class.mine]': 'tint() === "mine"',
    '[class.theirs]': 'tint() === "theirs"',
    '[attr.data-tone]': 'tone()',
    '[attr.aria-label]': 'label()',
    '(click)': 'select($event)',
  },
})

export class SheetRowStatusCell {
  public readonly rowId = input.required<number>();

  public readonly row = computed<SheetRow | null>(
    () => this.store.index().rows.get(this.rowId()) ?? null,
  );

  public readonly status = computed<ItemStatus | null>(() => {
    const row = this.row();
    return row === null ? null : ItemStatusUtil.itemStatus(row, this.store.publishing());
  });

  /** The row is tinted while someone has it, in the colour of whose it is. */
  public readonly tint = computed(() => {
    const lock = this.row()?.lock;
    if (!lock) {
      return null;
    }
    return lock.isMine ? 'mine' : 'theirs';
  });

  public readonly tone = computed<SectionTone>(() =>
    SheetIndexUtil.sectionTone(this.store.index(), this.rowId()),
  );

  public readonly isSelected = computed(() => this.store.isRowSelected(this.rowId())());

  /** The row changed after the version being compared against. */
  public readonly changed = computed(
    () => this.store.markedChange(this.row()?.lastChange) !== null,
  );

  /**
   * The row's state and who has it ("Draft · A. Smith"), which the narrow column shows only as an icon,
   * over when the row's version was published and by whom ("v3 · 12 Sep 2026 · A. Smith").
   */
  public readonly detail = computed(() => {
    const status = this.status();
    const change = this.row()?.lastChange;
    const lines = [
      status === null ? null : ItemStatusUtil.stateText(status),
      change ? SheetLabelsUtil.changeLabel(change) : null,
    ].filter((line) => line !== null);
    return lines.length > 0 ? lines.join('\n') : undefined;
  });

  public readonly label = computed(() => {
    const status = this.status();
    return status === null ? null : `Status: ${ItemStatusUtil.statusText(status)}`;
  });

  private readonly store = inject(SheetStore);

  public select(event: Event): void {
    event.stopPropagation();
    this.store.selectRow(this.rowId());
  }
}
