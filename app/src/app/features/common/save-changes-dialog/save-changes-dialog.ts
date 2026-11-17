import { DialogRef } from '@angular/cdk/dialog';
import { Component } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { DeactivationType } from '../../../core/types/deactivation-type.enum';
import { DialogHeader } from '../dialog-header/dialog-header';

@Component({
  selector: 'app-save-changes-dialog',
  imports: [ButtonDirective, DialogHeader],
  templateUrl: './save-changes-dialog.html',
  styleUrl: './save-changes-dialog.scss',
  host: { class: 'dialog card' },
})
export class SaveChangesDialog {
  constructor(
    private readonly _dialogRef: DialogRef<DeactivationType, SaveChangesDialog>,
  ) {
    this._dialogRef.disableClose = true;
  }

  public close(result: DeactivationType): void {
    this._dialogRef.close(result);
  }
}
