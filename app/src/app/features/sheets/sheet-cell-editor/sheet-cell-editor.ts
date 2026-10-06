import { Component, computed, ElementRef, inject, input, output } from '@angular/core';
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

/** The panels PrimeNG controls open outside the cell (a dropdown's list, a calendar). Using one is not leaving. */
const CONTROL_PANELS = '.p-select-overlay, .p-datepicker-panel, .p-overlay';

/**
 * The control for a value cell, chosen by the cell type's kind. It is the one place that maps a kind to
 * its editor: to support a new kind, add its editor component and a `@case` here.
 */
@Component({
  selector: 'app-sheet-cell-editor',
  imports: [SheetCheckboxCell, SheetDateCell, SheetDropdownCell, SheetNumberCell, SheetTextCell],
  templateUrl: './sheet-cell-editor.html',
  styleUrl: './sheet-cell-editor.scss',
  host: {
    '(document:pointerdown)': 'noticeLeaving($event)',
    '(document:focusin)': 'noticeLeaving($event)',
  },
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
  /** The user pressed or moved focus somewhere outside the cell and the panels its control opens. */
  public readonly left = output<void>();

  /** Which editor to show. Text and number dropdowns share one. */
  public readonly editor = computed(() => {
    const kind = this.cellType().kind;
    return isDropdown(kind) ? 'Dropdown' : kind;
  });

  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);

  /**
   * A press or a focus move anywhere on the page, while this editor is on screen. Only an editor that
   * is on screen listens, so this costs nothing for the cells showing plain values.
   */
  public noticeLeaving(event: Event): void {
    const target = event.target;
    if (!(target instanceof Element)) {
      return;
    }
    const cell = this.element.nativeElement.closest('[role="gridcell"]');
    if (!cell?.contains(target) && target.closest(CONTROL_PANELS) === null) {
      this.left.emit();
    }
  }
}
