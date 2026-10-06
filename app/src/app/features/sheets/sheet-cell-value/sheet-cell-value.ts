import { Component, computed, input } from '@angular/core';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellType } from '../../templates/models/cell-type.model';
import { displayValue } from '../cell-value.util';
import { SheetCell } from '../models/sheet-cell.model';

/**
 * A value cell that can't be changed (a past version, or a row someone else is editing), as plain
 * content: a tick mark for a checkbox, the value as text, or the caption as a hint when it is empty.
 */
@Component({
  selector: 'app-sheet-cell-value',
  templateUrl: './sheet-cell-value.html',
  styleUrl: './sheet-cell-value.scss',
})
export class SheetCellValue {
  public readonly cell = input.required<SheetCell | null>();
  public readonly cellType = input.required<CellType | null>();
  /** The cell type's settings with the cell's own overrides applied. */
  public readonly configuration = input.required<CellConfiguration | null>();
  /** Shown in place of a value the cell doesn't have. */
  public readonly hint = input('');

  public readonly isCheckbox = computed(() => this.cellType()?.kind === 'Checkbox');

  public readonly isChecked = computed(() => this.cell()?.booleanValue === true);

  public readonly text = computed(() => {
    const cell = this.cell();
    const cellType = this.cellType();
    const configuration = this.configuration();
    return cell !== null && cellType !== null && configuration !== null
      ? displayValue(cell, cellType, configuration)
      : '';
  });
}
