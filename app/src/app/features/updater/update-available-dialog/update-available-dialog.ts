import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, Inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Globals } from '../../../globals';
import { DialogHeader } from '../../common/dialog-header/dialog-header';
import { NgSwAppData } from './data/ngsw-app-data';
import { UpdateDialogResult } from './results/update-dialog-result';

@Component({
  selector: 'app-update-available-dialog',
  imports: [DialogHeader, ButtonDirective],
  templateUrl: './update-available-dialog.html',
  styleUrl: './update-available-dialog.scss',
  host: { class: 'dialog card' },
})
export class UpdateAvailableDialog {
  public readonly currentVersion: string;
  public readonly latestVersion: string;

  constructor(
    @Inject(DIALOG_DATA) private readonly _data: NgSwAppData,
    private readonly _dialogRef: DialogRef<
      UpdateDialogResult,
      UpdateAvailableDialog
    >,
  ) {
    _dialogRef.disableClose = true;

    this.currentVersion = Globals.AppVersion;
    this.latestVersion = this._data.version;
  }

  public update() {
    this._dialogRef.close({
      action: 'update',
    });
  }

  public defer() {
    this._dialogRef.close({
      action: 'defer',
    });
  }
}
