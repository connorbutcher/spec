import { Component, computed, inject, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { SelectButtonModule } from 'primeng/selectbutton';
import { LinkedDropdownInstanceSettings } from '../models/linked-dropdown-instance-settings.model';
import { SheetLabelsUtil } from '../status/sheet-labels.util';
import { SheetStore } from '../sheet.store';

/** A list this long gets a search box. */
const FILTER_FROM = 8;

/**
 * Where a linked dropdown cell takes its choices from: a table on the sheet, then either one of its
 * columns (the cell offers that column's value from every row) or one of its rows (the cell offers every
 * value in that row). Nothing is saved until Apply; Clear leaves the cell with no source at all.
 */
@Component({
  selector: 'app-sheet-linked-source-form',
  imports: [ButtonModule, FormsModule, SelectButtonModule, SelectModule],
  templateUrl: './sheet-linked-source-form.html',
  styleUrl: './sheet-linked-source-form.scss',
})

export class SheetLinkedSourceForm {
  /** What the cell is pointed at now, or null when nothing has been chosen. */
  public readonly settings = input.required<LinkedDropdownInstanceSettings | null>();

  /** The source to point the cell at, or null to clear it. */
  public readonly applied = output<LinkedDropdownInstanceSettings | null>();
  public readonly cancelled = output<void>();

  public readonly parts = [
    { value: 'column', label: 'A column' },
    { value: 'row', label: 'A row' },
  ];

  public readonly tableId = linkedSignal<number | null>(
    () => this.settings()?.sourceSheetTableId ?? null,
  );

  /** Whether the choices are a column's values or a row's. */
  public readonly part = linkedSignal<'column' | 'row'>(() =>
    (this.settings()?.sourceSheetRowId ?? null) === null ? 'column' : 'row',
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

  /** Starts as the cell's row, and is cleared whenever another table is picked. */
  public readonly rowId = linkedSignal<number | null, number | null>({
    source: this.tableId,
    computation: (tableId) => {
      const settings = this.settings();
      return settings !== null && settings.sourceSheetTableId === tableId
        ? (settings.sourceSheetRowId ?? null)
        : null;
    },
  });

  /** Every table on the sheet that has something to offer. */
  public readonly tables = computed(() =>
    [...this.store.index().tables.values()]
      .filter((table) => table.linkableColumns.length > 0 || table.linkableRows.length > 0)
      .map((table) => ({ id: table.id, label: SheetLabelsUtil.tableLabel(table) })),
  );

  public readonly columns = computed(() => this.columnsOf(this.tableId()));

  public readonly rows = computed(() => {
    const tableId = this.tableId();
    return tableId === null ? [] : (this.store.index().tables.get(tableId)?.linkableRows ?? []);
  });

  public readonly hasRowFilter = computed(() => this.rows().length > FILTER_FROM);

  public readonly hasSettings = computed(() => this.settings() !== null);

  /** What Apply would save, or null while the table or its column or row is still to be chosen. */
  public readonly chosen = computed<LinkedDropdownInstanceSettings | null>(() => {
    const sourceSheetTableId = this.tableId();
    const isRow = this.part() === 'row';
    const sourceTemplateCellId = isRow ? null : this.templateCellId();
    const sourceSheetRowId = isRow ? this.rowId() : null;
    return sourceSheetTableId === null ||
      (sourceTemplateCellId === null && sourceSheetRowId === null)
      ? null
      : { kind: 'LinkedDropdown', sourceSheetTableId, sourceTemplateCellId, sourceSheetRowId };
  });

  /** Something is chosen and it differs from what the cell already has. */
  public readonly canApply = computed(() => {
    const chosen = this.chosen();
    const settings = this.settings();
    return (
      chosen !== null &&
      (settings?.sourceSheetTableId !== chosen.sourceSheetTableId ||
        (settings.sourceTemplateCellId ?? null) !== chosen.sourceTemplateCellId ||
        (settings.sourceSheetRowId ?? null) !== chosen.sourceSheetRowId)
    );
  });

  private readonly store = inject(SheetStore);

  public apply(): void {
    const chosen = this.chosen();
    if (chosen !== null && this.canApply()) {
      this.applied.emit(chosen);
    }
  }

  private columnsOf(tableId: number | null) {
    return tableId === null ? [] : (this.store.index().tables.get(tableId)?.linkableColumns ?? []);
  }
}
