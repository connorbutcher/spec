import { Component, computed, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ItemStatusUtil } from '../status/item-status.util';
import { ItemStatus } from '../models/item-status.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetContextActions } from '../sheet-context-actions/sheet-context-actions';
import { SheetContextAdd } from '../sheet-context-add/sheet-context-add';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetLabelsUtil } from '../status/sheet-labels.util';
import { SheetLockUtil } from '../checkouts/sheet-lock.util';
import { SheetStatus } from '../sheet-status/sheet-status';
import { SheetTakeoverButton } from '../sheet-takeover-button/sheet-takeover-button';
import { SheetStore } from '../sheet.store';

/**
 * The one bar for whatever is selected on the sheet, under the sheet's top line and above every table.
 * It says where the selection is (table › sections › row, each step selecting that level), where the
 * selected row stands (its version, state and who has it), and holds the add, move and remove actions.
 * It stays put while the tables scroll, so the actions are always in the same place.
 */
@Component({
  selector: 'app-sheet-context-bar',
  imports: [
    BreadcrumbModule,
    SheetContextActions,
    SheetContextAdd,
    SheetStatus,
    SheetTakeoverButton,
  ],
  templateUrl: './sheet-context-bar.html',
  styleUrl: './sheet-context-bar.scss',
})

export class SheetContextBar {
  public readonly row = computed<SheetRow | null>(() => {
    const rowId = this.store.selection()?.rowId ?? null;
    return rowId === null ? null : (this.store.index().rows.get(rowId) ?? null);
  });

  /** Limits › Group › Limits › Stem diameter. Each step but the row selects that table or section. */
  public readonly path = computed<MenuItem[]>(() => {
    const selection = this.store.selection();
    const index = this.store.index();
    const table = index.tables.get(selection?.tableId ?? -1);
    if (selection === null || table === undefined) {
      return [];
    }
    const steps: MenuItem[] = [
      {
        label: SheetLabelsUtil.tableLabel(table),
        command: () => this.store.select({ tableId: table.id, sectionId: null, rowId: null }),
      },
    ];
    const section = index.sections.get(selection.sectionId ?? -1);
    if (section !== undefined) {
      for (const step of [...SheetIndexUtil.sectionAncestors(index, section.id), section]) {
        steps.push({ label: SheetLabelsUtil.sectionLabel(step), command: () => this.store.selectSection(step.id) });
      }
    }
    const row = this.row();
    if (row !== null) {
      steps.push({ label: SheetLabelsUtil.rowLabel(row) ?? 'Row' });
    }
    return steps;
  });

  public readonly hasSelection = computed(() => this.path().length > 0);

  /** Where the selected row stands, written as it is in the status column but never cut short. */
  public readonly rowStatus = computed<ItemStatus | null>(() => {
    const row = this.row();
    return row === null ? null : ItemStatusUtil.itemStatus(row, this.store.publishing());
  });

  public readonly lockedByOther = computed(() => SheetLockUtil.otherUsersLock(this.row()?.lock) !== null);

  /** Someone else has the row and it can be asked for. One waiting for approval stays with its author. */
  public readonly canAskForRow = computed(
    () => this.lockedByOther() && this.canEdit() && this.row()?.lock?.isAwaitingApproval !== true,
  );

  public readonly canEdit = computed(() => this.store.canEdit());

  private readonly store = inject(SheetStore);
}
