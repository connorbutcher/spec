import { Dialog } from '@angular/cdk/dialog';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { SpecSheetHelpers } from '../../../../../../core/helpers/spec-sheet-helpers';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { SpinnerDialogService } from '../../../../../../core/services/spinner-dialog.service';
import { ConfirmationDialog } from '../../../../../common/confirmation-dialog/confirmation-dialog';
import { ConfirmationDialogData } from '../../../../../common/confirmation-dialog/models/confirmation-dialog-data.interface';
import { PropertySummaryValueDisplay } from '../../../../../properties/components/display/property-summary-value-display/property-summary-value-display';
import { SheetsAuditService } from '../../sheets-audit.service';
import { SheetListItem } from '../../../../../../core/models/domain/sheets/sheet-list-item';

@Component({
  selector: 'app-sheet-for-phase-row',
  imports: [
    Tag,
    PropertySummaryValueDisplay,
    Tooltip,
    DatePipe,
    ButtonDirective,
  ],
  templateUrl: './sheet-for-phase-row.html',
  styleUrl: './sheet-for-phase-row.scss',
})
export class SheetForPhaseRow {
  public readonly sheet = input.required<SheetListItem>();
  public readonly phase = input.required<SpecSheetPhase>();
  public readonly type = input.required<SpecSheetType>();

  public readonly properties = computed(() => {
    const sheet = this.sheet();
    const type = this.type();
    if (!sheet || !type) {
      return [];
    }

    return SpecSheetHelpers.getPropertiesForSpecSheetSummary(sheet, type);
  });

  private readonly _messageService = inject(MessageService);
  private readonly _auditService = inject(SheetsAuditService);
  private readonly _spinnerDialogService = inject(SpinnerDialogService);
  private readonly _dialog = inject(Dialog);

  public async deleteSpecSheet(): Promise<void> {
    if (
      !this.sheet() ||
      !this.phase() ||
      !this.type() ||
      !this.sheet().isDraft
    ) {
      return;
    }

    const confirmedRef = this._dialog.open<
      boolean,
      ConfirmationDialogData,
      ConfirmationDialog
    >(ConfirmationDialog, {
      data: {
        title: 'Delete Spec Sheet',
        message: `Are you sure you want to delete this spec sheet? This action cannot be undone.`,
        confirmText: 'Delete',
        confirmSeverity: 'danger',
      },
    });

    const confirmed = await firstValueFrom(confirmedRef.closed);
    if (!confirmed) {
      return;
    }

    const spinnerRef = this._spinnerDialogService.open(
      'Deleting Spec Sheet',
      'Please wait while the spec sheet is being deleted.',
    );

    try {
      await this._auditService.deleteDraftAsync(this.sheet().sheetId);

      this._messageService.add({
        severity: 'success',
        summary: 'Spec Sheet Deleted',
        detail: 'The spec sheet has been successfully deleted.',
      });
      this._auditService.specSheetsResource.reload();
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: 'Delete Failed',
        detail: 'An error occurred while deleting the spec sheet.',
      });
    } finally {
      spinnerRef.close();
    }
  }
}
