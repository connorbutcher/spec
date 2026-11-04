import { Component, computed, inject } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SheetActionTarget } from '../models/sheet-action-target.model';
import { SheetActionTargetUtil } from '../sheet-action-target.util';
import { SheetConfirmUtil } from '../sheet-confirm.util';
import { SheetStore } from '../sheet.store';

/**
 * Move up, move down and remove for whatever is selected on the sheet: the row, else its section, else
 * the table (see `sheetActionTarget`). One set serves every table, and each button says what it does.
 */
@Component({
  selector: 'app-sheet-context-actions',
  imports: [ButtonModule],
  templateUrl: './sheet-context-actions.html',
  styleUrl: './sheet-context-actions.scss',
})

export class SheetContextActions {
  public readonly target = computed<SheetActionTarget | null>(() =>
    SheetActionTargetUtil.sheetActionTarget(this.store.index(), this.store.sheet()?.tables ?? [], this.store.selection()),
  );

  /** "Remove row": the button names what it will take, as the selection can be any of three things. */
  public readonly removeLabel = computed(() => {
    const target = this.target();
    return target === null ? 'Remove' : `Remove ${target.kind}`;
  });

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly store = inject(SheetStore);
  private readonly confirmation = inject(ConfirmationService);

  public move(step: -1 | 1): void {
    const target = this.target();
    if (target === null) {
      return;
    }
    switch (target.kind) {
      case 'row':
        void this.store.moveRow(target.id, step);
        break;
      case 'section':
        void this.store.moveSection(target.id, step);
        break;
      case 'table':
        void this.store.moveTableTo(target.id, this.tablePosition(target.id) + step);
        break;
    }
  }

  public askRemove(event: Event): void {
    const target = this.target();
    if (target !== null) {
      SheetConfirmUtil.confirmRemoval(this.confirmation, event, target.removeQuestion, () => this.remove(target));
    }
  }

  private remove(target: SheetActionTarget): Promise<void> {
    switch (target.kind) {
      case 'row':
        return this.store.removeRow(target.id);
      case 'section':
        return this.store.removeSection(target.id);
      case 'table':
        return this.store.removeTable(target.id);
    }
  }

  private tablePosition(tableId: number): number {
    return (this.store.sheet()?.tables ?? []).findIndex((table) => table.id === tableId);
  }
}
