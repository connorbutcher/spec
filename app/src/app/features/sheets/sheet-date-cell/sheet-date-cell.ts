import { Component, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { fromDateValue, toDateValue, valueRequest } from '../cell-value.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';

/** Keeps the date's own alignment and makes the PrimeNG input fill the cell. */
const INPUT_STYLE = { height: '100%', 'text-align': 'inherit' };

/**
 * The editor for a date cell: a PrimeNG date picker that fills the whole cell. A date can be typed
 * (`yyyy-mm-dd`) or picked, and is saved when it's picked, cleared, or focus leaves the box.
 */
@Component({
  selector: 'app-sheet-date-cell',
  imports: [DatePickerModule, FormsModule],
  templateUrl: './sheet-date-cell.html',
  styleUrl: './sheet-date-cell.scss',
})
export class SheetDateCell {
  public readonly cell = input.required<SheetCell>();
  /** The cell's caption, shown as a hint while the cell is empty. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly date = linkedSignal<Date | null>(() => fromDateValue(this.cell().dateValue));

  public readonly inputStyle = INPUT_STYLE;

  public pick(value: Date | null): void {
    this.date.set(value);
    this.commit();
  }

  public commit(): void {
    const date = this.date();
    const value = date === null ? null : toDateValue(date);
    if (value !== this.cell().dateValue) {
      this.changed.emit(valueRequest(this.cell(), 'Date', date));
    }
  }
}
