import { Component, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

/**
 * A PrimeNG number input that saves when you leave it or press Enter, not on every keystroke. Emits
 * the new value only when it changed; blank emits null when `allowEmpty` is set.
 */
@Component({
  selector: 'app-number-field',
  imports: [FormsModule, InputNumberModule],
  templateUrl: './number-field.html',
  styleUrl: './number-field.scss',
})
export class NumberField {
  public readonly value = input.required<number | null>();
  public readonly inputId = input<string>();
  public readonly min = input<number>();
  public readonly max = input<number>();
  /** Decimal places allowed; 0 for whole numbers. */
  public readonly decimals = input(0);
  public readonly allowEmpty = input(false);
  public readonly placeholder = input('');
  public readonly suffix = input('');
  public readonly disabled = input(false);

  public readonly committed = output<number | null>();

  /** What's typed so far; resets whenever the saved value changes. */
  public readonly draft = linkedSignal<number | null>(() => this.value());

  public commit(): void {
    const draft = this.draft();
    if (draft === null && !this.allowEmpty()) {
      this.draft.set(this.value());
      return;
    }
    if (draft !== this.value()) {
      this.committed.emit(draft);
    }
  }
}
