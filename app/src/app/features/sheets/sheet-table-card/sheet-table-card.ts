import { CdkDragHandle } from '@angular/cdk/drag-drop';
import { Component, computed, inject, input, linkedSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { SheetTable } from '../models/sheet-table.model';
import { SheetActionBar } from '../sheet-action-bar/sheet-action-bar';
import { SheetAddMenu } from '../sheet-add-menu/sheet-add-menu';
import { confirmRemoval } from '../sheet-confirm.util';
import { SheetGrid } from '../sheet-grid/sheet-grid';
import { tableLabel } from '../sheet-labels.util';
import { otherUsersLock } from '../sheet-lock.util';
import { SheetStore } from '../sheet.store';

/**
 * One table on the sheet: its title (typed in by the user), the sections that can be added to it, its
 * own move and remove actions, the actions for the current selection, and the grid itself. A table can
 * be moved by dragging its handle or with its up and down buttons, which are the keyboard's way.
 */
@Component({
  selector: 'app-sheet-table-card',
  imports: [
    ButtonModule,
    CdkDragHandle,
    FormsModule,
    InputTextModule,
    SheetActionBar,
    SheetAddMenu,
    SheetGrid,
    TagModule,
  ],
  templateUrl: './sheet-table-card.html',
  styleUrl: './sheet-table-card.scss',
})
export class SheetTableCard {
  public readonly table = input.required<SheetTable>();

  public readonly label = computed(() => tableLabel(this.table()));

  /** The title as it is being typed. It is saved when focus leaves the box; Escape puts the saved one back. */
  public readonly title = linkedSignal(() => this.table().title ?? '');

  public readonly canEdit = computed(() => this.store.canEdit());

  /** The title and position can be changed unless someone else has a change to them in progress. */
  public readonly canEditTable = computed(
    () => this.store.canEdit() && otherUsersLock(this.table().lock) === null,
  );

  public readonly lockedBy = computed(() => otherUsersLock(this.table().lock)?.userName ?? null);

  public readonly isBusy = computed(() => this.store.isBusy());

  /** Where the table sits among the sheet's tables, counted from 0. */
  public readonly position = computed(() =>
    (this.store.sheet()?.tables ?? []).findIndex((table) => table.id === this.table().id),
  );

  public readonly canMoveUp = computed(() => this.position() > 0);

  public readonly canMoveDown = computed(() => {
    const position = this.position();
    return position >= 0 && position < (this.store.sheet()?.tables.length ?? 0) - 1;
  });

  private readonly store = inject(SheetStore);
  private readonly confirmation = inject(ConfirmationService);

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

  public move(step: -1 | 1): void {
    void this.store.moveTableTo(this.table().id, this.position() + step);
  }

  public askRemove(event: Event): void {
    confirmRemoval(
      this.confirmation,
      event,
      `Remove ${this.label()}? Everything in it goes too.`,
      () => this.store.removeTable(this.table().id),
    );
  }
}
