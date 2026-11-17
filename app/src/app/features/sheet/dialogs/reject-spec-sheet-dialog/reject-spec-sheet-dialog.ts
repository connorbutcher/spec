import { DialogRef } from '@angular/cdk/dialog';
import { Component, inject, Signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { ElementLabel } from '../../../common/element-label/element-label';
import { RichTextEditor } from '../../../common/rich-text-editor/rich-text-editor';
import { PUSpecSheet } from '../../models/pu-spec-sheet';
import { ActiveSheetService } from '../../services/active-sheet.service';

@Component({
  selector: 'app-reject-spec-sheet-dialog',
  imports: [
    ReactiveFormsModule,
    DialogHeader,
    ElementLabel,
    ButtonDirective,
    RichTextEditor,
  ],
  templateUrl: './reject-spec-sheet-dialog.html',
  styleUrl: './reject-spec-sheet-dialog.scss',
  host: { class: 'dialog card' },
})
export class RejectSpecSheetDialog {
  public readonly reasonCtrl = new FormControl('', [Validators.required]);

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _dialogRef = inject(
    DialogRef<string, RejectSpecSheetDialog>,
  );

  public confirm(): void {
    this._dialogRef.close(this.reasonCtrl.value!);
  }

  public close(): void {
    this._dialogRef.close(undefined);
  }

  public hasError(error: string): boolean {
    return this.reasonCtrl.dirty && this.reasonCtrl.hasError(error);
  }

  public get specSheet(): Signal<PUSpecSheet> {
    return this._activeSheetService.specSheet;
  }
}
