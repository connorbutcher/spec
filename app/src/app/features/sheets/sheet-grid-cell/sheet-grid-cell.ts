import { Component, computed, inject, input } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { effectiveConfiguration, effectiveStyle } from '../../templates/cell-settings.util';
import { cellStyleCss } from '../../templates/cell-style-css.util';
import { isDisplayOnly } from '../../templates/models/cell-kinds';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { CellType } from '../../templates/models/cell-type.model';
import { GridStyle } from '../../templates/models/grid-style';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetCellEditor } from '../sheet-cell-editor/sheet-cell-editor';
import { SheetStore } from '../sheet.store';

const FLEX_ALIGNMENT: Readonly<Record<string, string>> = {
  left: 'flex-start',
  center: 'center',
  right: 'flex-end',
};

/**
 * A cell on the sheet grid, placed by the template layout. Heading and group cells show their caption.
 * Other cells show their value, or, in a row the user has locked, a PrimeNG editor for it. A row locked
 * by someone else is marked with a lock and can't be edited. Clicking selects the cell's row; double
 * clicking (or Enter) locks it for editing.
 */
@Component({
  selector: 'app-sheet-grid-cell',
  imports: [SheetCellEditor, TooltipModule],
  templateUrl: './sheet-grid-cell.html',
  styleUrl: './sheet-grid-cell.scss',
  host: {
    role: 'gridcell',
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    '[class.mine]': 'isMine()',
    '[class.theirs]': 'lockedByOther()',
    '[class.display-only]': 'displayOnly()',
    '[attr.tabindex]': 'isEditing() ? null : 0',
    '(click)': 'select($event)',
    '(dblclick)': 'startEditing()',
    '(keydown.enter)': 'startEditing()',
  },
})
export class SheetGridCell {
  public readonly layout = input.required<CellLayout>();

  public readonly cell = computed<SheetCell | null>(
    () => this.store.index().cells.get(this.layout().cell.id) ?? null,
  );

  public readonly row = computed<SheetRow | null>(
    () => this.store.index().rows.get(this.layout().rowId) ?? null,
  );

  public readonly cellType = computed<CellType | null>(
    () => this.store.cellTypes().get(this.layout().cell.cellTypeId) ?? null,
  );

  public readonly displayOnly = computed(() => isDisplayOnly(this.cellType()?.kind));

  public readonly configuration = computed(() => {
    const cellType = this.cellType();
    return cellType
      ? effectiveConfiguration(cellType.configuration, this.layout().cell.configurationOverride)
      : null;
  });

  public readonly styleCss = computed<Record<string, string>>(() => {
    const cellType = this.cellType();
    return cellType
      ? cellStyleCss(effectiveStyle(cellType.style, this.layout().cell.styleOverride))
      : {};
  });

  public readonly isMine = computed(() => this.row()?.lock?.isMine === true);

  public readonly lockedByOther = computed(() => {
    const lock = this.row()?.lock;
    return lock !== null && lock !== undefined && !lock.isMine;
  });

  /** Whether the cell shows an editor: its row is locked by the user, on the live sheet. */
  public readonly isEditing = computed(
    () => this.store.canEdit() && this.isMine() && !this.displayOnly() && this.cellType() !== null,
  );

  public readonly isSelected = computed(
    () => this.store.selection()?.rowId === this.layout().rowId,
  );

  /** The lock marker goes on the first cell of the row only. */
  public readonly showsLock = computed(() => {
    const row = this.row();
    return row !== null && row.lock !== null && row.cells[0]?.id === this.layout().cell.id;
  });

  public readonly lockLabel = computed(() => {
    const lock = this.row()?.lock;
    return lock?.isMine
      ? 'You are editing this row'
      : `Being edited by ${lock?.userName ?? 'someone'}`;
  });

  public readonly caption = computed(() => this.layout().cell.caption ?? '');

  /** What a cell shows when it isn't being edited. */
  public readonly display = computed(() => {
    const cell = this.cell();
    const cellType = this.cellType();
    const configuration = this.configuration();
    if (cell === null || cellType === null || configuration === null) {
      return '';
    }
    switch (cellType.kind) {
      case 'Text': {
        return cell.textValue ?? '';
      }
      case 'Number': {
        return this.formatNumber(
          cell.numberValue,
          'decimalPlaces' in configuration ? configuration.decimalPlaces : null,
          'unit' in configuration ? configuration.unit : null,
        );
      }
      case 'Date': {
        return cell.dateValue ?? '';
      }
      case 'TextDropdown':
      case 'NumberDropdown': {
        return cellType.options.find((option) => option.id === cell.optionId)?.value ?? '';
      }
      default: {
        return '';
      }
    }
  });

  public readonly isCheckbox = computed(() => this.cellType()?.kind === 'Checkbox');

  public readonly isChecked = computed(() => this.cell()?.booleanValue === true);

  private readonly store = inject(SheetStore);

  /** The grid placement plus the cell's background, which fills the whole grid area. */
  public hostStyle(): GridStyle {
    const background = this.styleCss()['background-color'];
    return background
      ? { ...this.layout().style, 'background-color': background }
      : this.layout().style;
  }

  /** The cell's text styling, with its alignment also lining up the caption and value. */
  public contentStyle(): Record<string, string> {
    const { 'background-color': _background, ...text } = this.styleCss();
    const align = text['text-align'];
    return align ? { ...text, 'align-items': FLEX_ALIGNMENT[align] ?? 'flex-start' } : text;
  }

  public select(event: Event): void {
    event.stopPropagation();
    this.store.selectRow(this.layout().rowId);
  }

  public startEditing(): void {
    const row = this.row();
    if (!this.store.canEdit() || this.displayOnly() || row === null || row.lock !== null) {
      return;
    }
    this.store.selectRow(row.id);
    void this.store.lockRow(row.id);
  }

  public save(request: CellValueRequest): void {
    const row = this.row();
    if (row !== null) {
      void this.store.saveValues(row.id, [request]);
    }
  }

  private formatNumber(
    value: number | null,
    decimals: number | null | undefined,
    unit: string | null | undefined,
  ): string {
    if (value === null) {
      return '';
    }
    const text =
      decimals === null || decimals === undefined ? String(value) : value.toFixed(decimals);
    return unit ? `${text} ${unit}` : text;
  }
}
