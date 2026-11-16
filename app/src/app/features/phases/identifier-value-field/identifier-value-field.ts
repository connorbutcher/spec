import { Component, computed, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { SheetTypeIdentifier } from '../../../core/models/sheet-type-identifier.model';
import { CellValueUtil } from '../../sheets/values/cell-value.util';

/**
 * One identifier's value in the New sheet dialog, with the PrimeNG control for its kind. The value is
 * always text, as the API takes it: a number as typed, a date as yyyy-MM-dd, an option by its name.
 */
@Component({
  selector: 'app-identifier-value-field',
  imports: [DatePickerModule, FormsModule, InputNumberModule, InputTextModule, SelectModule],
  templateUrl: './identifier-value-field.html',
  styleUrl: './identifier-value-field.scss',
})

export class IdentifierValueField {
  public readonly identifier = input.required<SheetTypeIdentifier>();
  public readonly value = model('');

  public readonly inputId = computed(() => `sheet-identifier-${this.identifier().id}`);

  public readonly number = computed<number | null>(() => {
    const value = this.value();
    return value === '' || Number.isNaN(Number(value)) ? null : Number(value);
  });

  public readonly date = computed<Date | null>(() => CellValueUtil.fromDateValue(this.value() || null));

  public setNumber(value: number | null): void {
    this.value.set(value === null ? '' : String(value));
  }

  public setDate(value: Date | null): void {
    this.value.set(value === null ? '' : CellValueUtil.toDateValue(value));
  }

  public setText(value: string | null): void {
    this.value.set(value ?? '');
  }
}
