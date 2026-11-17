import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, Inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { ConfirmationDialogData } from './models/confirmation-dialog-data.interface';

@Component({
  selector: 'app-confirmation-dialog',
  templateUrl: './confirmation-dialog.html',
  styleUrl: './confirmation-dialog.scss',
  imports: [ButtonDirective],
  host: { class: 'dialog card' },
})
export class ConfirmationDialog {
  public title: string;
  public message: string;
  public confirmButtonText: string | undefined;
  public cancelButtonText: string | undefined;
  public showCancelButton: boolean;
  public confirmSeverity: 'success' | 'info' | 'help' | 'danger' | undefined;

  constructor(
    private readonly _dialogRef: DialogRef<boolean, ConfirmationDialog>,
    @Inject(DIALOG_DATA)
    private readonly _dialogConfig: ConfirmationDialogData,
  ) {
    this._dialogRef.disableClose = true;

    this.title = this._dialogConfig.title;
    this.message = this._dialogConfig.message;
    this.confirmButtonText = this._dialogConfig.confirmText;
    this.cancelButtonText = this._dialogConfig.cancelText;
    this.confirmSeverity = this._dialogConfig.confirmSeverity;
    this.showCancelButton =
      this._dialogConfig.showCancelButton === undefined
        ? true
        : this._dialogConfig.showCancelButton;
  }

  public close(confirm: boolean): void {
    this._dialogRef.close(confirm);
  }
}
