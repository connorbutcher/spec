import { Component, computed, input, output } from '@angular/core';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { isDropdown } from '../../templates/models/cell-kinds';
import { CellType } from '../../templates/models/cell-type.model';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetCheckboxCell } from '../sheet-checkbox-cell/sheet-checkbox-cell';
import { SheetDateCell } from '../sheet-date-cell/sheet-date-cell';
import { SheetDropdownCell } from '../sheet-dropdown-cell/sheet-dropdown-cell';
import { SheetNumberCell } from '../sheet-number-cell/sheet-number-cell';
import { SheetTextCell } from '../sheet-text-cell/sheet-text-cell';

/**
 * The control for a value cell, chosen by the cell type's kind. It is the one place that maps a kind to
 * its editor: to support a new kind, add its editor component and a `@case` here.
 */
@Component({
  selector: 'app-sheet-cell-editor',
  imports: [SheetCheckboxCell, SheetDateCell, SheetDropdownCell, SheetNumberCell, SheetTextCell],
  templateUrl: './sheet-cell-editor.html',
  styleUrl: './sheet-cell-editor.scss',
})
export class SheetCellEditor {
  public readonly cell = input.required<SheetCell>();
  public readonly cellType = input.required<CellType>();
  /** The cell type's settings with the cell's own overrides applied. */
  public readonly configuration = input.required<CellConfiguration>();
  /** The cell's caption, shown as a hint while the cell is empty. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  /** Which editor to show. Text and number dropdowns share one. */
  public readonly editor = computed(() => {
    const kind = this.cellType().kind;
    return isDropdown(kind) ? 'Dropdown' : kind;
  });
}
