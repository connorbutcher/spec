import { Dialog } from '@angular/cdk/dialog';
import { Component, computed, inject, Injector, input } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Tooltip } from 'primeng/tooltip';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { PUCardAnalyticsService } from '../../../analytics/services/pu-card-analytics.service';
import { PUSpecSheetAvatar } from '../../../common/pu-spec-sheet-avatar/pu-spec-sheet-avatar';
import { SpecSheetVersionHistoryDialog } from '../../../sheet/dialogs/spec-sheet-version-history-dialog/spec-sheet-version-history-dialog';
import { SheetsService } from '../../services/sheets.service';
import { SpecSheetVersionSelector } from '../spec-sheet-version-selector/spec-sheet-version-selector';

@Component({
  selector: 'app-sheet-actions',
  templateUrl: './sheet-actions.html',
  styleUrl: './sheet-actions.scss',
  imports: [
    SpecSheetVersionSelector,
    PUSpecSheetAvatar,
    ButtonDirective,
    ButtonGroup,
    Tooltip,
  ],
})
export class SheetActions {
  public readonly phase = input.required<SpecSheetPhase>();
  public readonly type = input.required<SpecSheetType>();
  public readonly isFirstVersion = computed(() => {
    const specSheet = this.selectedSpecSheet();
    const versions = this._sheetsService.specSheetsForPhaseAndType();
    if (versions.length === 0 || !specSheet) {
      return true;
    }

    return (
      versions[versions.length - 1].versionNumber === specSheet.versionNumber
    );
  });
  private readonly _sheetsService = inject(SheetsService);
  private readonly _puCardAnalyticsService = inject(PUCardAnalyticsService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);
  private readonly _router = inject(Router);

  public compareSpecSheets() {
    const specSheet = this.selectedSpecSheet();
    const versions = this._sheetsService.specSheetsForPhaseAndType();
    if (!specSheet || versions.length === 0) {
      return;
    }

    const indexOfCurrentVersion = versions.findIndex(
      (v) => v.versionNumber === specSheet.versionNumber,
    );
    if (indexOfCurrentVersion === -1 || indexOfCurrentVersion === 0) {
      return;
    }

    const previousVersion = versions[indexOfCurrentVersion - 1];
    if (!previousVersion) {
      return;
    }

    this._router.navigate(['comparison'], {
      queryParams: {
        type: this.type().id,
        phase: this.phase().id,
        left: previousVersion.versionNumber,
        right: specSheet.versionNumber,
      },
      queryParamsHandling: 'replace',
    });
  }

  public viewVersionHistory() {
    this._dialog.open(SpecSheetVersionHistoryDialog, {
      injector: this._injector,
    });
  }

  public get selectedSpecSheet() {
    return this._sheetsService.selectedSpecSheet;
  }

  public get isAnalyticsEnabled() {
    return this._puCardAnalyticsService.isEnabled;
  }
}
