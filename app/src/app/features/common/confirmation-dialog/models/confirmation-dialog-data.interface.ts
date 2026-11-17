export type ConfirmationDialogData = {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmSeverity?: 'success' | 'info' | 'help' | 'danger' | undefined;
  showCancelButton?: boolean;
};
