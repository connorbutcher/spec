import { Dialog } from '@angular/cdk/dialog';
import { inject, Injectable } from '@angular/core';
import { SpinnerDialogData } from '../../features/common/spinner-dialog/models/spinner-dialog-data';
import { SpinnerDialog } from '../../features/common/spinner-dialog/spinner-dialog';

@Injectable({ providedIn: 'root' })
export class SpinnerDialogService {
  private readonly _dialog = inject(Dialog);

  public open(message: string, hint: string) {
    return this._dialog.open<unknown, SpinnerDialogData, SpinnerDialog>(
      SpinnerDialog,
      {
        data: {
          name: message,
          hint,
        },
      },
    );
  }
}
