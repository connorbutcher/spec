import { CdkDragHandle } from '@angular/cdk/drag-drop';
import { Component, computed, inject, input, linkedSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { itemStatus } from '../item-status.util';
import { ItemStatus } from '../models/item-status.model';
import { SheetTable } from '../models/sheet-table.model';
import { SheetGrid } from '../sheet-grid/sheet-grid';
import { tableLabel } from '../sheet-labels.util';
import { otherUsersLock } from '../sheet-lock.util';
import { SheetStatus } from '../sheet-status/sheet-status';
import { SheetStore } from '../sheet.store';

/**
 * One table on the sheet: a single heading line (its title, typed in by the user, the template it was
 * built from and where it stands) and the grid itself. Clicking the heading selects the table, and the
 * sheet's context bar then adds to it, moves it or removes it. It can also be moved by dragging its handle.
 */
@Component({
  selector: 'app-sheet-table-card',
  imports: [CdkDragHandle, FormsModule, InputTextModule, SheetGrid, SheetStatus],
  templateUrl: './sheet-table-card.html',
  styleUrl: './sheet-table-card.scss',
})
export class SheetTableCard {
  public readonly table = input.required<SheetTable>();

  public readonly label = computed(() => tableLabel(this.table()));

  /** The title as it is being typed. It is saved when focus leaves the box; Escape puts the saved one back. */
  public readonly title = linkedSignal(() => this.table().title ?? '');

  /** The title and position can be changed unless someone else has a change to them in progress. */
  public readonly canEditTable = computed(
    () => this.store.canEdit() && otherUsersLock(this.table().lock) === null,
  );

  /** New, a draft and whose, or being published: where the table itself stands, beside its template's name. */
  public readonly status = computed<ItemStatus>(() =>
    itemStatus(this.table(), this.store.publishing()),
  );

  /** The table itself is selected, not a section or row in it. */
  public readonly isSelected = computed(() => {
    const selection = this.store.selection();
    return selection?.tableId === this.table().id && selection.sectionId === null;
  });

  private readonly store = inject(SheetStore);

  public selectTable(): void {
    this.store.select({ tableId: this.table().id, sectionId: null, rowId: null });
  }

  public rename(): void {
    const title = this.title().trim();
    if (title === (this.table().title ?? '')) {
      this.revertTitle();
      return;
    }
    void this.store.setTableTitle(this.table().id, title === '' ? null : title);
  }

  public revertTitle(): void {
    this.title.set(this.table().title ?? '');
  }
}
