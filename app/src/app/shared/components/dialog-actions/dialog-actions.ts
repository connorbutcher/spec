import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';

/**
 * The Cancel and confirm buttons at the foot of a dialog, popover or inline form. While `busy` is set,
 * Cancel is disabled and the confirm button spins.
 */
@Component({
  selector: 'app-dialog-actions',
  imports: [ButtonModule],
  templateUrl: './dialog-actions.html',
  styleUrl: './dialog-actions.scss',
})
export class DialogActions {
  public readonly confirmLabel = input.required<string>();
  public readonly confirmIcon = input<string>();
  public readonly confirmDisabled = input(false);
  public readonly busy = input(false);

  public readonly cancelled = output();
  public readonly confirmed = output();
}
