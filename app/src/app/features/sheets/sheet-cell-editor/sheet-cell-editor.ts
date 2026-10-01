import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellType } from '../../templates/models/cell-type.model';
import { fromDateValue, valueRequest } from '../cell-value.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';

/**
 * The PrimeNG control for one cell of a row the user has locked, chosen by the cell's kind and set up
 * from its effective configuration. Text and numbers are saved when the control loses focus, everything
 * else as soon as it changes. Nothing is emitted if the value hasn't changed.
 */
@Component({
  selector: 'app-sheet-cell-editor',
  imports: [
    CheckboxModule,
    DatePickerModule,
    FormsModule,
    InputNumberModule,
    InputTextModule,
    SelectModule,
    TextareaModule,
  ],
  templateUrl: './sheet-cell-editor.html',
  styleUrl: './sheet-cell-editor.scss',
})
export class SheetCellEditor {
  public readonly cell = input.required<SheetCell>();
  public readonly cellType = input.required<CellType>();
  /** The cell type's configuration with the cell's overrides laid on top. */
  public readonly configuration = input.required<CellConfiguration>();
  public readonly label = input<string>();

  public readonly changed = output<CellValueRequest>();

  public readonly text = linkedSignal(() => this.cell().textValue ?? '');
  public readonly number = linkedSignal<number | null>(() => this.cell().numberValue);
  public readonly date = linkedSignal<Date | null>(() => fromDateValue(this.cell().dateValue));
  public readonly checked = linkedSignal(() => this.cell().booleanValue ?? false);
  public readonly optionId = linkedSignal<number | null>(() => this.cell().optionId);

  public readonly kind = computed(() => this.cellType().kind);

  public readonly textSettings = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'Text' ? configuration : null;
  });

  public readonly numberSettings = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'Number' || configuration.kind === 'NumberDropdown'
      ? configuration
      : null;
  });

  /** The smallest and largest number a number cell accepts. Dropdown numbers pick from a list instead. */
  public readonly minValue = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'Number' ? (configuration.minValue ?? undefined) : undefined;
  });

  public readonly maxValue = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'Number' ? (configuration.maxValue ?? undefined) : undefined;
  });

  public readonly options = computed(() =>
    [...this.cellType().options].sort((a, b) => a.displayOrder - b.displayOrder),
  );

  /** The unit shown after a number, e.g. " Nm". */
  public readonly suffix = computed(() => {
    const unit = this.numberSettings()?.unit;
    return unit ? ` ${unit}` : '';
  });

  public commitText(): void {
    const value = this.text();
    if (value !== (this.cell().textValue ?? '')) {
      this.changed.emit(valueRequest(this.cell(), 'Text', value === '' ? null : value));
    }
  }

  public commitNumber(): void {
    const value = this.number();
    if (value !== this.cell().numberValue) {
      this.changed.emit(valueRequest(this.cell(), 'Number', value));
    }
  }

  public commitDate(value: Date | null): void {
    this.date.set(value);
    this.changed.emit(valueRequest(this.cell(), 'Date', value));
  }

  public commitChecked(value: boolean): void {
    this.checked.set(value);
    this.changed.emit(valueRequest(this.cell(), 'Checkbox', value));
  }

  public commitOption(value: number | null): void {
    this.optionId.set(value);
    this.changed.emit(valueRequest(this.cell(), this.kind(), value));
  }
}
