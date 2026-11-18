import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, Inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { ReferencesSidePanelService } from '../../features/references/references-side-panel/references-side-panel.service';
import { SystemReferencesConfirmationDialogData } from './models/system-references-confirmation-dialog-data.interface';
import { SheetExternalReference } from '../../../../core/models/domain/sheets/sheet';

@Component({
  selector: 'app-system-references-confirmation-dialog',
  templateUrl: './system-references-confirmation-dialog.html',
  styleUrl: './system-references-confirmation-dialog.scss',
  imports: [ButtonDirective],
  host: { class: 'dialog card' },
})
export class SystemReferencesConfirmationDialog {
  constructor(
    private readonly _dialogRef: DialogRef<
      boolean,
      SystemReferencesConfirmationDialog
    >,
    @Inject(DIALOG_DATA)
    private readonly _dialogConfig: SystemReferencesConfirmationDialogData,
    private readonly _referencesSidePanel: ReferencesSidePanelService,
  ) {
    this._dialogRef.disableClose = true;
  }

  public viewSystemReferences(): void {
    this.close(false);
    this._referencesSidePanel.open(this.references[0]?.referenceId);
  }

  public close(confirm: boolean): void {
    this._dialogRef.close(confirm);
  }

  public get type(): string {
    return this._dialogConfig.type;
  }

  public get references(): SheetExternalReference[] {
    return this._dialogConfig.references;
  }
}
