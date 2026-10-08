import { Component, computed, inject, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { LinkedDropdownInstanceSettings } from '../models/linked-dropdown-instance-settings.model';
import { tableLabel } from '../sheet-labels.util';
import { SheetStore } from '../sheet.store';

/**
 * Where a linked dropdown cell takes its choices from: a table on the sheet, then one of the columns that
 * table offers. Nothing is saved until Apply; Clear leaves the cell with no column at all.
 */
@Component({
  selector: 'app-sheet-linked-source-form',
  imports: [ButtonModule, FormsModule, SelectModule],
  templateUrl: './sheet-linked-source-form.html',
  styleUrl: './sheet-linked-source-form.scss',
})
export class SheetLinkedSourceForm {
  /** What the cell is pointed at now, or null when nothing has been chosen. */
  public readonly settings = input.required<LinkedDropdownInstanceSettings | null>();

  /** The column to point the cell at, or null to clear it. */
  public readonly applied = output<LinkedDropdownInstanceSettings | null>();
  public readonly cancelled = output<void>();

  public readonly tableId = linkedSignal<number | null>(
    () => this.settings()?.sourceSheetTableId ?? null,
  );

  /** Starts as the cell's column, and starts again whenever another table is picked. */
  public readonly templateCellId = linkedSignal<number | null, number | null>({
    source: this.tableId,
    computation: (tableId) => {
      const settings = this.settings();
      if (settings !== null && settings.sourceSheetTableId === tableId) {
        return settings.sourceTemplateCellId;
      }
      const columns = this.columnsOf(tableId);
      return columns.length === 1 ? columns[0].templateCellId : null;
    },
  });

  /** Every table on the sheet that has a column to offer. */
  public readonly tables = computed(() =>
    [...this.store.index().tables.values()]
      .filter((table) => table.linkableColumns.length > 0)
      .map((table) => ({ id: table.id, label: tableLabel(table) })),
  );

  public readonly columns = computed(() => this.columnsOf(this.tableId()));

  public readonly hasSettings = computed(() => this.settings() !== null);

  /** Both parts are chosen and they differ from what the cell already has. */
  public readonly canApply = computed(() => {
    const settings = this.settings();
    return (
      this.tableId() !== null &&
      this.templateCellId() !== null &&
      (settings?.sourceSheetTableId !== this.tableId() ||
        settings.sourceTemplateCellId !== this.templateCellId())
    );
  });

  private readonly store = inject(SheetStore);

  public apply(): void {
    if (this.canApply()) {
      this.applied.emit({
        kind: 'LinkedDropdown',
        sourceSheetTableId: this.tableId(),
        sourceTemplateCellId: this.templateCellId(),
      });
    }
  }

  private columnsOf(tableId: number | null) {
    return tableId === null ? [] : (this.store.index().tables.get(tableId)?.linkableColumns ?? []);
  }
}
