import { Component, computed, input } from '@angular/core';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellType } from '../../templates/models/cell-type.model';
import { displayValue } from '../cell-value.util';
import { SheetCell } from '../models/sheet-cell.model';

/**
 * A value cell as plain content: a tick mark for a checkbox, the value as text, or the caption as a
 * hint when it is empty. It is what a cell shows when it can't be changed (a past version, or a row
 * someone else is editing) and, for a cell that can, until the user moves into it.
 */
@Component({
  selector: 'app-sheet-cell-value',
  templateUrl: './sheet-cell-value.html',
  styleUrl: './sheet-cell-value.scss',
  host: {
    '[class.fills-cell]': 'fillsCell()',
    '[class.multiline]': 'isMultiline()',
    '[class.styled-by-cell]': 'isText()',
  },
})
export class SheetCellValue {
  public readonly cell = input.required<SheetCell | null>();
  public readonly cellType = input.required<CellType | null>();
  /** The cell type's settings with the cell's own overrides applied. */
  public readonly configuration = input.required<CellConfiguration | null>();
  /** Shown in place of a value the cell doesn't have. */
  public readonly hint = input('');
  /** Stands in for the cell's control, so it sits exactly where the control's own text would. */
  public readonly fillsCell = input(false);

  public readonly isCheckbox = computed(() => this.cellType()?.kind === 'Checkbox');

  public readonly isChecked = computed(() => this.cell()?.booleanValue === true);

  /** A text box takes its text style from the cell; the other controls keep their own. */
  public readonly isText = computed(() => this.cellType()?.kind === 'Text');

  /** A multi-line text cell wraps, as its text area does; anything else stays on one line, as its box does. */
  public readonly isMultiline = computed(() => {
    const configuration = this.configuration();
    return configuration?.kind === 'Text' && configuration.multiline === true;
  });

  public readonly text = computed(() => {
    const cell = this.cell();
    const cellType = this.cellType();
    const configuration = this.configuration();
    return cell !== null && cellType !== null && configuration !== null
      ? displayValue(cell, cellType, configuration)
      : '';
  });
}
