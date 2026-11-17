import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, Inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { SheetCheckedOutDialogData } from './models/sheet-checked-out-dialog-data';

@Component({
  selector: 'app-sheet-checked-out-dialog',
  templateUrl: './sheet-checked-out-dialog.html',
  styleUrl: './sheet-checked-out-dialog.scss',
  imports: [ButtonDirective],
  host: { class: 'dialog card' },
})
export class SheetCheckedOutDialog {
  public title: string;
  public message: string;

  constructor(
    private readonly _dialogRef: DialogRef<
      boolean | undefined,
      SheetCheckedOutDialog
    >,
    @Inject(DIALOG_DATA)
    private readonly _dialogConfig: SheetCheckedOutDialogData,
  ) {
    this._dialogRef.disableClose = true;

    this.title = this._dialogConfig.title;
    this.message = this._dialogConfig.message;
  }

  public close(confirm?: boolean): void {
    this._dialogRef.close(confirm);
  }
}
