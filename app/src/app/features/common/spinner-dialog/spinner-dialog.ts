import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, Inject } from '@angular/core';
import { Spinner } from '../spinner/spinner';
import { SpinnerDialogData } from './models/spinner-dialog-data';

@Component({
  selector: 'app-spinner-dialog',
  templateUrl: './spinner-dialog.html',
  styleUrl: './spinner-dialog.scss',
  host: { class: 'card dialog' },
  imports: [Spinner],
})
export class SpinnerDialog {
  public readonly name: string;
  public readonly hint: string;

  constructor(
    private readonly _dialogRef: DialogRef,
    @Inject(DIALOG_DATA) private readonly _dialogConfig: SpinnerDialogData,
  ) {
    this._dialogRef.disableClose = true;

    this.name = this._dialogConfig.name;
    this.hint = this._dialogConfig.hint;
  }
}
