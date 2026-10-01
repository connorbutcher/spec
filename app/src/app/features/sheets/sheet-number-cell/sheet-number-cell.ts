import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { valueRequest } from '../cell-value.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';

/** Keeps the number's own alignment and makes the PrimeNG input fill the cell. */
const INPUT_STYLE = { height: '100%', 'text-align': 'inherit' };

/**
 * The editor for a number cell: a PrimeNG number box that fills the whole cell, set up with the cell's
 * decimal places, limits and unit. The value is saved when focus leaves it or on Enter; Escape puts
 * back the saved value. Nothing is emitted if the number hasn't changed.
 */
@Component({
  selector: 'app-sheet-number-cell',
  imports: [FormsModule, InputNumberModule],
  templateUrl: './sheet-number-cell.html',
  styleUrl: './sheet-number-cell.scss',
  host: {
    '(keydown.escape)': 'revert()',
  },
})
export class SheetNumberCell {
  public readonly cell = input.required<SheetCell>();
  public readonly configuration = input.required<CellConfiguration>();
  /** The cell's caption, shown as a hint while the cell is empty. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly number = linkedSignal<number | null>(() => this.cell().numberValue);

  public readonly inputStyle = INPUT_STYLE;

  public readonly settings = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'Number' ? configuration : null;
  });

  /** The unit shown after the number, e.g. " Nm". */
  public readonly suffix = computed(() => {
    const unit = this.settings()?.unit;
    return unit ? ` ${unit}` : '';
  });

  public commit(): void {
    const value = this.number();
    if (value !== this.cell().numberValue) {
      this.changed.emit(valueRequest(this.cell(), 'Number', value));
    }
  }

  public revert(): void {
    this.number.set(this.cell().numberValue);
  }
}
