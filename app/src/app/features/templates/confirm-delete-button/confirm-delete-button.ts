import { Component, inject, input, output } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';

/**
 * A delete button that asks for confirmation in a PrimeNG confirm popup before emitting. Needs a
 * `ConfirmationService` and a `<p-confirmpopup>` from a parent (the templates page provides both).
 */
@Component({
  selector: 'app-confirm-delete-button',
  imports: [ButtonModule],
  templateUrl: './confirm-delete-button.html',
  styleUrl: './confirm-delete-button.scss',
})
export class ConfirmDeleteButton {
  public readonly label = input.required<string>();
  /** What will go with it, shown in the popup, e.g. "Its rows and cells go too." */
  public readonly warning = input<string>();
  public readonly disabled = input(false);

  public readonly confirmed = output<void>();

  private readonly confirmation = inject(ConfirmationService);

  public ask(event: Event): void {
    this.confirmation.confirm({
      target: event.currentTarget as EventTarget,
      message: [`${this.label()}?`, this.warning()].filter(Boolean).join(' '),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonProps: { severity: 'danger', size: 'small' },
      rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
      accept: () => this.confirmed.emit(),
    });
  }
}
