import { Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CheckboxChangeEvent, CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { SelectButtonChangeEvent, SelectButtonModule } from 'primeng/selectbutton';
import { ColorField } from '../color-field/color-field';
import { SettingField } from '../models/setting-field.model';
import { SettingValue } from '../models/setting-value';
import { NumberField } from '../number-field/number-field';

/**
 * The PrimeNG control for one configuration or style setting, picked by the field's control type.
 * Emits the new value once it's committed; null means cleared.
 */
@Component({
  selector: 'app-setting-control',
  imports: [
    CheckboxModule,
    ColorField,
    FormsModule,
    InputTextModule,
    NumberField,
    SelectButtonModule,
  ],
  templateUrl: './setting-control.html',
  styleUrl: './setting-control.scss',
})
export class SettingControl {
  public readonly field = input.required<SettingField>();
  public readonly value = input.required<SettingValue>();
  public readonly inputId = input.required<string>();
  public readonly disabled = input(false);

  public readonly committed = output<SettingValue>();

  public readonly numberValue = computed(() => {
    const value = this.value();
    return typeof value === 'number' ? value : null;
  });

  public readonly textValue = computed(() => {
    const value = this.value();
    return typeof value === 'string' ? value : null;
  });

  public readonly decimals = computed(() =>
    this.field().control === 'decimal' ? (this.field().decimals ?? 4) : 0,
  );

  public commitText(input: HTMLInputElement): void {
    const value = input.value.trim() || null;
    if (value !== this.textValue()) {
      this.committed.emit(value);
    }
    input.value = this.textValue() ?? '';
  }

  public commitBoolean(event: CheckboxChangeEvent): void {
    this.committed.emit(event.checked === true);
  }

  public commitChoice(event: SelectButtonChangeEvent): void {
    this.committed.emit((event.value as string | null | undefined) ?? null);
  }
}
