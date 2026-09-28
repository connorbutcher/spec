import { Component, input, output, signal } from '@angular/core';

/** A delete button that asks "Delete? Yes / Cancel" inline before emitting. */
@Component({
  selector: 'app-confirm-delete-button',
  templateUrl: './confirm-delete-button.html',
  styleUrl: './confirm-delete-button.scss',
})
export class ConfirmDeleteButton {
  public readonly label = input.required<string>();
  /** What will go with it, shown while confirming, e.g. "Its rows and cells go too." */
  public readonly warning = input<string>();
  public readonly disabled = input(false);

  public readonly confirmed = output<void>();

  public readonly isConfirming = signal(false);

  public ask(): void {
    this.isConfirming.set(true);
  }

  public cancel(): void {
    this.isConfirming.set(false);
  }

  public confirm(): void {
    this.isConfirming.set(false);
    this.confirmed.emit();
  }
}
