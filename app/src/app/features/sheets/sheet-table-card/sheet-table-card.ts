import { CdkDragHandle } from '@angular/cdk/drag-drop';
import { Component, computed, inject, input } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { SheetTable } from '../models/sheet-table.model';
import { SheetActionBar } from '../sheet-action-bar/sheet-action-bar';
import { SheetAddMenu } from '../sheet-add-menu/sheet-add-menu';
import { SheetGrid } from '../sheet-grid/sheet-grid';
import { SheetStore } from '../sheet.store';

/**
 * One table on the sheet: its title (typed in by the user), the sections that can be added to it, its
 * own move and remove actions, the actions for the current selection, and the grid itself.
 */
@Component({
  selector: 'app-sheet-table-card',
  imports: [
    ButtonModule,
    CdkDragHandle,
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

  public readonly label = computed(() => this.table().title || this.table().templateName);

  public readonly canEdit = computed(() => this.store.canEdit());

  /** The title and position can be changed unless someone else has a change to them in progress. */
  public readonly canEditTable = computed(
    () =>
      this.store.canEdit() && (this.table().lock === null || this.table().lock?.isMine === true),
  );

  public readonly lockedBy = computed(() => {
    const lock = this.table().lock;
    return lock !== null && !lock.isMine ? lock.userName : null;
  });

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly store = inject(SheetStore);
  private readonly confirmation = inject(ConfirmationService);

  public selectTable(): void {
    this.store.select({ tableId: this.table().id, sectionId: null, rowId: null });
  }

  public rename(input: HTMLInputElement): void {
    const title = input.value.trim();
    if (title !== (this.table().title ?? '')) {
      void this.store.setTableTitle(this.table().id, title === '' ? null : title);
    }
  }

  public askRemove(event: Event): void {
    this.confirmation.confirm({
      target: event.currentTarget as EventTarget,
      message: `Remove ${this.label()}? Everything in it goes too.`,
      acceptLabel: 'Remove',
      rejectLabel: 'Cancel',
      acceptButtonProps: { size: 'small', severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
      accept: () => void this.store.removeTable(this.table().id),
    });
  }
}
