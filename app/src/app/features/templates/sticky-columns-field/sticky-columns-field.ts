import { Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { NumberField } from '../number-field/number-field';

/**
 * The "sticky columns" setting: a toggle that pins columns from the left while the rest scroll sideways,
 * and, when it's on, how many columns from the left are pinned. Turning it on pins the first column.
 */
@Component({
  selector: 'app-sticky-columns-field',
  imports: [FormsModule, NumberField, ToggleSwitchModule],
  templateUrl: './sticky-columns-field.html',
  styleUrl: './sticky-columns-field.scss',
})
export class StickyColumnsField {
  public readonly id = input.required<string>();

  /** What the setting is for, e.g. "Sticky columns". */
  public readonly label = input.required<string>();

  /** How many columns are pinned now; 0 means none. */
  public readonly count = input.required<number>();

  /** How many columns there are to pin. */
  public readonly available = input(50);

  public readonly disabled = input(false);

  public readonly changed = output<number>();

  public readonly isOn = computed(() => this.count() > 0);

  public toggle(on: boolean): void {
    this.changed.emit(on ? 1 : 0);
  }

  public setCount(value: number | null): void {
    this.changed.emit(Math.max(1, value ?? 1));
  }
}
