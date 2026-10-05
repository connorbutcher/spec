import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { CellType } from '../../templates/models/cell-type.model';
import { valueRequest } from '../cell-value.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';

/**
 * The editor for a text or number dropdown cell: a PrimeNG select, filling the whole cell, of the cell
 * type's options. The choice is saved as soon as it's made, and can be cleared.
 */
@Component({
  selector: 'app-sheet-dropdown-cell',
  imports: [FormsModule, SelectModule],
  templateUrl: './sheet-dropdown-cell.html',
  styleUrl: './sheet-dropdown-cell.scss',
})
export class SheetDropdownCell {
  public readonly cell = input.required<SheetCell>();
  public readonly cellType = input.required<CellType>();
  /** The cell's caption, shown as a hint while nothing is chosen. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly optionId = linkedSignal<number | null>(() => this.cell().optionId);

  public readonly options = computed(() =>
    [...this.cellType().options].sort((a, b) => a.displayOrder - b.displayOrder),
  );

  public choose(value: number | null): void {
    this.optionId.set(value);
    if (value !== this.cell().optionId) {
      this.changed.emit(valueRequest(this.cell(), this.cellType().kind, value));
    }
  }
}
