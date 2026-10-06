import { ConfirmationService } from 'primeng/api';

/**
 * Asks whether to remove something, in the confirm popup beside the control that was used (the sheet
 * page holds the one `p-confirmpopup`). `remove` runs only if the user confirms.
 */
export function confirmRemoval(
  confirmation: ConfirmationService,
  event: Event,
  message: string,
  remove: () => Promise<void>,
): void {
  confirmation.confirm({
    target: event.currentTarget as EventTarget,
    message,
    acceptLabel: 'Remove',
    rejectLabel: 'Cancel',
    acceptButtonProps: { size: 'small', severity: 'danger' },
    rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
    accept: () => void remove(),
  });
}
