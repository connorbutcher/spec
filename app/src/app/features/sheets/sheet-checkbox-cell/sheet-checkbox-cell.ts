import { Component, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';
import { valueRequest } from '../cell-value.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';

/**
 * The editor for a checkbox cell. Clicking anywhere in the cell ticks or unticks it, and the PrimeNG
 * checkbox can also be reached with the keyboard (Space). Every change is saved straight away.
 */
@Component({
  selector: 'app-sheet-checkbox-cell',
  imports: [CheckboxModule, FormsModule],
  templateUrl: './sheet-checkbox-cell.html',
  styleUrl: './sheet-checkbox-cell.scss',
})
export class SheetCheckboxCell {
  public readonly cell = input.required<SheetCell>();
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly checked = linkedSignal(() => this.cell().booleanValue ?? false);

  /** A click on the cell's empty space. Clicks on the checkbox itself and key presses go through its own change event. */
  public toggleFromCell(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.started.emit();
      this.set(!this.checked());
    }
  }

  public set(value: boolean): void {
    this.checked.set(value);
    this.changed.emit(valueRequest(this.cell(), 'Checkbox', value));
  }
}
