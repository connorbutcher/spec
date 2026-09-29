import { Component, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ColorPickerModule } from 'primeng/colorpicker';

/**
 * A PrimeNG colour picker that saves when the picker closes, not on every drag. Shows the hex value,
 * and a clear button that emits null.
 */
@Component({
  selector: 'app-color-field',
  imports: [ButtonModule, ColorPickerModule, FormsModule],
  templateUrl: './color-field.html',
  styleUrl: './color-field.scss',
})
export class ColorField {
  public readonly value = input.required<string | null>();
  public readonly inputId = input<string>();
  public readonly disabled = input(false);

  public readonly committed = output<string | null>();

  /** The colour being picked; resets whenever the saved value changes. */
  public readonly draft = linkedSignal<string | null>(() => this.value());

  public commit(): void {
    const draft = this.draft();
    if (draft !== this.value()) {
      this.committed.emit(draft);
    }
  }

  public clear(): void {
    this.committed.emit(null);
  }
}
