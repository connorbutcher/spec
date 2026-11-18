import { DialogRef } from '@angular/cdk/dialog';
import { Component, computed, inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { SheetsService } from '../../../sheets/services/sheets.service';
import { SpecSheetVersionHistoryRow } from './components/spec-sheet-version-history-row/spec-sheet-version-history-row';

@Component({
  selector: 'app-spec-sheet-version-history-dialog',
  imports: [DialogHeader, ButtonDirective, SpecSheetVersionHistoryRow],
  templateUrl: './spec-sheet-version-history-dialog.html',
  styleUrl: './spec-sheet-version-history-dialog.scss',
  host: { class: 'dialog card' },
})
export class SpecSheetVersionHistoryDialog {
  public readonly historyData = computed(() => {
    const specSheetVersions = this._sheetsService.specSheetsForPhaseAndType();
    if (!specSheetVersions || specSheetVersions.length === 0) {
      return [];
    }

    return specSheetVersions.filter((v) => !v.isDraft);
  });

  private readonly _sheetsService = inject(SheetsService);
  private readonly _dialogRef = inject(DialogRef);

  public close(): void {
    this._dialogRef.close();
  }

  public get specSheetType() {
    return this._sheetsService.selectedType;
  }
}
